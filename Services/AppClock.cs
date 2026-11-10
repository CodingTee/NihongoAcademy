namespace NihongoAcademy.Services
{
    public interface IAppClock
    {
        DateTime Now { get; }
        DateTime Today { get; }
    }

    public sealed class AppClock : IAppClock
    {
        private readonly TimeZoneInfo _timeZone;

        public AppClock(IConfiguration configuration)
        {
            var configuredId = configuration["App:TimeZoneId"] ?? "Singapore Standard Time";
            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuredId);
            }
            catch (TimeZoneNotFoundException)
            {
                _timeZone = TimeZoneInfo.CreateCustomTimeZone("UTC+08", TimeSpan.FromHours(8), "UTC+08", "UTC+08");
            }
            catch (InvalidTimeZoneException)
            {
                _timeZone = TimeZoneInfo.CreateCustomTimeZone("UTC+08", TimeSpan.FromHours(8), "UTC+08", "UTC+08");
            }
        }

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

        public DateTime Today => Now.Date;
    }
}
