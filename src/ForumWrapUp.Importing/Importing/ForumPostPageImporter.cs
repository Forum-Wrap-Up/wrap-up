using ForumFree.Net.Models;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Importing.ForumFree;
using Microsoft.Extensions.Options;
using ApiPost = ForumFree.Net.Models.UserPost;

namespace ForumWrapUp.Importing;

public sealed class ForumPostPageImporter
{
    private readonly IForumFreeApiClient _forumApiClient;
    private readonly ForumImportOperationManager _operationManager;
    private readonly TimeSpan _maximumPageDelay;

    public ForumPostPageImporter(IForumFreeApiClient forumApiClient, ForumImportOperationManager operationManager, IOptions<ForumImportApiOptions> options)
    {
        _forumApiClient = forumApiClient;
        _operationManager = operationManager;
        _maximumPageDelay = options.Value.MaximumPageDelay;

        if (_maximumPageDelay < TimeSpan.Zero)
            throw new InvalidOperationException("ForumApi:MaximumPageDelay cannot be negative.");
    }

    public async Task<ForumPostImportBatch> FetchAsync(ImportOperation operation, ForumImportDateRange dateRange, CancellationToken cancellationToken)
    {
        Dictionary<int, ApiPost> postsInRange = [];
        int pageNumber = 1;
        int? expectedPages = null;
        int? expectedTotal = null;
        int? apiForumId = null;

        while (expectedPages is null || pageNumber <= expectedPages.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserPostsResponse response = await _forumApiClient.GetPostsByUserIdAsync(operation.Mid, pageNumber);
            ValidatePage(response, pageNumber, expectedPages, expectedTotal, apiForumId);

            if (expectedPages is null)
            {
                expectedPages = response.Pages;
                expectedTotal = response.Total;
                apiForumId = response.IdForum;
            }

            if (response.Member is not null && response.Member.Id != operation.Mid)
                throw new InvalidDataException("The forum API returned posts for a different MID.");

            foreach (ApiPost post in response.Posts)
            {
                DateTime publishedAtUtc = post.Date.UtcDateTime;
                if (publishedAtUtc < dateRange.FromUtc || publishedAtUtc >= dateRange.ToUtcExclusive)
                    continue;

                postsInRange[post.Id] = post;
            }

            await _operationManager.ReportPageFetchedAsync(operation, response, cancellationToken);

            if (expectedPages == 0 || pageNumber >= expectedPages.Value)
                break;

            await DelayBeforeNextPageAsync(cancellationToken);
            pageNumber++;
        }

        if (!apiForumId.HasValue)
            throw new InvalidDataException("The forum API did not identify the source forum.");

        return new ForumPostImportBatch(
            apiForumId.Value,
            postsInRange);
    }

    private static void ValidatePage(UserPostsResponse response, int requestedPage, int? expectedPages, int? expectedTotal, int? apiForumId)
    {
        if (response.IdForum <= 0)
            throw new InvalidDataException("The forum API returned an invalid source forum ID.");

        if (response.Pages < 0 || response.Total < 0)
            throw new InvalidDataException("The forum API returned invalid pagination totals.");

        if (response.Pages == 0 && (requestedPage != 1 || response.Total != 0 || response.Posts.Count != 0))
            throw new InvalidDataException("The forum API returned inconsistent empty pagination metadata.");

        if (response.Pages > 0 && response.CurrentPage != requestedPage)
            throw new InvalidDataException("The forum API returned a page other than the requested page.");

        if (expectedPages.HasValue && response.Pages != expectedPages.Value)
            throw new InvalidDataException("The number of forum API pages changed during import.");

        if (expectedTotal.HasValue && response.Total != expectedTotal.Value)
            throw new InvalidDataException("The forum API result count changed during import.");

        if (apiForumId.HasValue && response.IdForum != apiForumId.Value)
            throw new InvalidDataException("The forum API source changed during import.");
    }

    private async Task DelayBeforeNextPageAsync(CancellationToken cancellationToken)
    {
        if (_maximumPageDelay == TimeSpan.Zero)
            return;

        long delayTicks = Random.Shared.NextInt64(0, _maximumPageDelay.Ticks);
        TimeSpan delay = TimeSpan.FromTicks(delayTicks);
        await Task.Delay(delay, cancellationToken);
    }
}
