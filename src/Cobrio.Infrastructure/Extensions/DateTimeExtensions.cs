namespace Cobrio.Infrastructure.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo BrasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    /// <summary>
    /// Converte DateTime UTC para horário de Brasília (UTC-3)
    /// </summary>
    public static DateTime ToBrasiliaTime(this DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, BrasiliaTimeZone);
    }

    /// <summary>
    /// Converte DateTime nullable UTC para horário de Brasília (UTC-3)
    /// </summary>
    public static DateTime? ToBrasiliaTime(this DateTime? utcDateTime)
    {
        return utcDateTime?.ToBrasiliaTime();
    }

    /// <summary>
    /// Converte horário de Brasília para UTC
    /// </summary>
    public static DateTime ToUtc(this DateTime brasiliaDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(brasiliaDateTime, BrasiliaTimeZone);
    }
}
