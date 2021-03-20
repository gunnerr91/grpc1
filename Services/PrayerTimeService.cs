using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PrayerApiContract;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GRPC1
{
    public class PrayerTimeService : prayertime.prayertimeBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<PrayerTimeService> logger;

        public PrayerTimeService(IHttpClientFactory httpClientFactory, ILogger<PrayerTimeService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public override async Task<ResponseForPrayerTimesInCity> GetPrayerTimesByCity(
            RequestForPrayerTimesInCity request,
            ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();
            PrayerTimes prayerTimes = await GetPrayerTimes(request, client);

            return new ResponseForPrayerTimesInCity
            {
                Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
        }

        public override async Task GetPrayerTimesByCityStream(
            RequestForPrayerTimesInCity request,
            IServerStreamWriter<ResponseForPrayerTimesInCity> responseStream,
            ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();

            for (int i = 0; i < 30; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("service request cancelled...");
                    break;
                }

                var prayerTimes = await GetPrayerTimes(request, client);
                await responseStream.WriteAsync(new ResponseForPrayerTimesInCity
                {
                    Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                    Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                    Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                    Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                    Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                    TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow)
                });
                await Task.Delay(1000);
            }
        }

        private static async Task<PrayerTimes> GetPrayerTimes(RequestForPrayerTimesInCity request, HttpClient client)
        {
            var response = await client.GetStringAsync($"https://api.pray.zone/v2/times/today.json?city={request.City}&school={request.School}");

            var prayerTimes = PrayerTimes.FromJson(response);
            return prayerTimes;
        }
    }
}
