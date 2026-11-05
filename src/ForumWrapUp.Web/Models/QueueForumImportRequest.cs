namespace ForumWrapUp.Web.Models;

public sealed class QueueForumImportRequest
{
    public required int Mid { get; init; }

    public required DateOnly FromDate { get; init; }

    public required DateOnly ToDate { get; init; }
}
