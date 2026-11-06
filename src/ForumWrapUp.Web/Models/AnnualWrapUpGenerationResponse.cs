using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Web.Models;

public sealed class AnnualWrapUpGenerationResponse
{
    public int Mid { get; }

    public int Year { get; }

    public int TotalPosts { get; }

    public long TotalPoints { get; }

    public int? TopTopicExternalId { get; }

    public int TopTopicPostCount { get; }

    public int? MostSuccessfulPostExternalId { get; }

    public int? MostSuccessfulPostPoints { get; }

    public DateTime? CalculatedAtUtc { get; }

    public AnnualWrapUpGenerationResponse(int mid, int year, int totalPosts, long totalPoints, int? topTopicExternalId,
    int topTopicPostCount, int? mostSuccessfulPostExternalId, int? mostSuccessfulPostPoints, DateTime? calculatedAtUtc)
    {
        Mid = mid;
        Year = year;
        TotalPosts = totalPosts;
        TotalPoints = totalPoints;
        TopTopicExternalId = topTopicExternalId;
        TopTopicPostCount = topTopicPostCount;
        MostSuccessfulPostExternalId = mostSuccessfulPostExternalId;
        MostSuccessfulPostPoints = mostSuccessfulPostPoints;
        CalculatedAtUtc = calculatedAtUtc;
    }

    public static AnnualWrapUpGenerationResponse FromSummary(AnnualWrapUp summary)
    {
        return new AnnualWrapUpGenerationResponse(
            summary.Mid,
            summary.Year,
            summary.TotalPosts,
            summary.TotalPoints,
            summary.TopTopicExternalId,
            summary.TopTopicPostCount,
            summary.MostSuccessfulPostExternalId,
            summary.MostSuccessfulPostPoints,
            summary.CalculatedAtUtc);
    }
}
