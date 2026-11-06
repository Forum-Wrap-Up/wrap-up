namespace ForumWrapUp.Importing;

public static class ForumLocalTime
{
    private static readonly TimeZoneInfo RomeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");

    public static DateTime ToUtc(DateTime localDateTime)
    {
        DateTime unspecifiedLocalDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecifiedLocalDateTime, RomeTimeZone);
    }

    public static DateOnly ToDate(DateTime utcDateTime)
    {
        DateTime normalizedUtcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(normalizedUtcDateTime, RomeTimeZone);
        return DateOnly.FromDateTime(localDateTime);
    }
}
