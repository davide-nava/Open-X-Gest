using OpenX.Gest.Application.Common.Interfaces;

namespace OpenX.Gest.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo SwissTimeZone = GetSwissTimeZone();

    private static TimeZoneInfo GetSwissTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime SwissNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SwissTimeZone);

    public DateTime ToSwissTime(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, SwissTimeZone);
    }

    public DateTime ToUtc(DateTime swissDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(swissDateTime, SwissTimeZone);
    }
}
