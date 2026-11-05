using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Web.Models;

public sealed class ForumImportOperationResponse
{
    public required Guid OperationId { get; init; }

    public required string Status { get; init; }

    public required string StatusUrl { get; init; }

    public required int Mid { get; init; }

    public required DateOnly FromDate { get; init; }

    public required DateOnly ToDate { get; init; }

    public required DateTime RequestedAtUtc { get; init; }

    public DateTime? StartedAtUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public required int PagesFetched { get; init; }

    public int? TotalPagesExpected { get; init; }

    public required int PostsFetched { get; init; }

    public int? TotalPostsExpected { get; init; }

    public string? ErrorMessage { get; init; }

    public static ForumImportOperationResponse FromOperation(ImportOperation operation, string statusUrl)
    {
        return new ForumImportOperationResponse
        {
            OperationId = operation.Id,
            Status = operation.Status.ToString().ToLowerInvariant(),
            StatusUrl = statusUrl,
            Mid = operation.Mid,
            FromDate = operation.FromDate,
            ToDate = operation.ToDate,
            RequestedAtUtc = operation.RequestedAtUtc,
            StartedAtUtc = operation.StartedAtUtc,
            CompletedAtUtc = operation.CompletedAtUtc,
            PagesFetched = operation.PagesFetched,
            TotalPagesExpected = operation.TotalPagesExpected,
            PostsFetched = operation.PostsFetched,
            TotalPostsExpected = operation.TotalPostsExpected,
            ErrorMessage = operation.ErrorMessage
        };
    }
}
