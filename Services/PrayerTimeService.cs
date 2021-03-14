using Grpc.Core;
using PrayerApiContract;
using System.Net.Http;
using System.Threading.Tasks;

namespace GRPC1
{
    public class PrayerTimeService : prayertime.prayertimeBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        public PrayerTimeService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public override async Task<ResponseForPrayerTimesInCity> GetPrayerTimesByCity(RequestForPrayerTimesInCity request, ServerCallContext context)
        {
            var client = httpClientFactory.CreateClient();

            var response = await client.GetStringAsync($"https://api.pray.zone/v2/times/today.json?city={request.City}&school={request.School}");

            var prayerTimes = PrayerTimes.FromJson(response);

            return new ResponseForPrayerTimesInCity
            {
                Asr = prayerTimes.Results.Datetime[0].Times.Asr,
                Dhuhr = prayerTimes.Results.Datetime[0].Times.Dhuhr,
                Fajr = prayerTimes.Results.Datetime[0].Times.Fajr,
                Isha = prayerTimes.Results.Datetime[0].Times.Isha,
                Maghrib = prayerTimes.Results.Datetime[0].Times.Maghrib
            };
        }
    }
}
