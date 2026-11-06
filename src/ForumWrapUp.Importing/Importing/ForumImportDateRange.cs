namespace ForumWrapUp.Importing;

public sealed record ForumImportDateRange
{
    public ForumImportDateRange(DateTime fromUtc, DateTime toUtcExclusive)
    {
        FromUtc = fromUtc;
        ToUtcExclusive = toUtcExclusive;
    }

    public DateTime FromUtc { get; }

    public DateTime ToUtcExclusive { get; }

    public static ForumImportDateRange Create(DateOnly fromDate, DateOnly toDate)
    {
        if (fromDate > toDate)
            throw new ArgumentException("The start date must not be after the end date.");

        if (toDate == DateOnly.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(toDate), "The end date is out of range.");

        DateTime fromLocal = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        DateOnly dayAfterToDate = toDate.AddDays(1);
        DateTime toLocalExclusive = dayAfterToDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        DateTime fromUtc = ForumLocalTime.ToUtc(fromLocal);
        DateTime toUtcExclusive = ForumLocalTime.ToUtc(toLocalExclusive);

        return new ForumImportDateRange(fromUtc, toUtcExclusive);
    }
}
