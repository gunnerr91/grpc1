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

        public override async Task<ResponseForPrayerTimesInCityAndSchool> GetPrayerTimesByCityAndSchool(RequestForPrayerTimesInCityAndSchool request, ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();
            PrayerTimes prayerTimes = await GetPrayerTimesWithSchool(request, client);

            return new ResponseForPrayerTimesInCityAndSchool
            {
                Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                City = request.City,
                School = request.School
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

        public override async Task GetPrayerTimesByCityAndSchoolStream(RequestForPrayerTimesInCityAndSchool request, IServerStreamWriter<ResponseForPrayerTimesInCityAndSchool> responseStream, ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();

            for (int i = 0; i < 30; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("service request cancelled...");
                    break;
                }

                var prayerTimes = await GetPrayerTimesWithSchool(request, client);
                await responseStream.WriteAsync(new ResponseForPrayerTimesInCityAndSchool
                {
                    Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                    Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                    Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                    Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                    Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                    TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    School = request.School,
                    City = request.City
                });
                await Task.Delay(1000);
            }
        }

        public override async Task<ResponseForPrayerTimesForMultipleCities> GetMultplePrayerTimesBasedOnCity(IAsyncStreamReader<RequestForPrayerTimesInCity> requestStream, ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();
            var response = new ResponseForPrayerTimesForMultipleCities
            {
                PrayerTimes = { }
            };
            await foreach (var request in requestStream.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("service request cancelled...");
                    break;
                }

                var prayerTimes = await GetPrayerTimes(request, client);
                response.PrayerTimes.Add(new ResponseForPrayerTimesInCity
                {
                    Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                    Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                    Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                    Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                    Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                    TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    City = request.City
                });
            }
            return response;
        }

        public override async Task<ResponseForPrayerTimesForMultipleCitiesAndSchools> GetMultplePrayerTimesBasedOnCityAndSchool(IAsyncStreamReader<RequestForPrayerTimesInCityAndSchool> requestStream, ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();
            var response = new ResponseForPrayerTimesForMultipleCitiesAndSchools
            {
                PrayerTimes = { }
            };
            await foreach (var request in requestStream.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("service request cancelled...");
                    break;
                }

                var prayerTimes = await GetPrayerTimesWithSchool(request, client);
                response.PrayerTimes.Add(new ResponseForPrayerTimesInCityAndSchool
                {
                    Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                    Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                    Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                    Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                    Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib,
                    TimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    City = request.City,
                    School = request.School
                });
            }
            return response;
        }

        private static async Task<PrayerTimes> GetPrayerTimes(RequestForPrayerTimesInCity request, HttpClient client)
        {
            var response = await client.GetStringAsync($"https://api.pray.zone/v2/times/today.json?city={request.City}");

            var prayerTimes = PrayerTimes.FromJson(response);
            return prayerTimes;
        }

        private static async Task<PrayerTimes> GetPrayerTimesWithSchool(RequestForPrayerTimesInCityAndSchool request, HttpClient client)
        {
            var response = await client.GetStringAsync($"https://api.pray.zone/v2/times/today.json?city={request.City}&school={(int)request.School}");

            var prayerTimes = PrayerTimes.FromJson(response);
            return prayerTimes;
        }
    }
}
