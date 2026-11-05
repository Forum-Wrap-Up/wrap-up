using System.Globalization;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities.Raw;
using Microsoft.EntityFrameworkCore;
using ApiPost = ForumFree.Net.Models.UserPost;

namespace ForumWrapUp.Importing;

public sealed class ForumRawPostSynchronizer
{
    private readonly ForumWrapUpDbContext _dbContext;

    public ForumRawPostSynchronizer(ForumWrapUpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SynchronizeAsync(int mid, ForumPostImportBatch batch, ForumRawReferences references, ForumImportDateRange dateRange, CancellationToken cancellationToken)
    {
        int[] importedPostIds = batch.PostsById.Keys.ToArray();
        Dictionary<int, ForumPost> existingPosts = importedPostIds.Length == 0
            ? []
            : await _dbContext.ForumPosts
                .Where(post =>
                    post.ApiForumId == batch.ApiForumId &&
                    importedPostIds.Contains(post.ExternalId))
                .ToDictionaryAsync(post => post.ExternalId, cancellationToken);

        foreach (KeyValuePair<int, ApiPost> item in batch.PostsById)
        {
            ApiPost apiPost = item.Value;
            existingPosts.TryGetValue(apiPost.Id, out ForumPost? existingPost);

            if (existingPost is not null && existingPost.AuthorMid != mid)
                throw new InvalidDataException("The forum API returned a post already associated with another MID.");

            ForumTopic? topic = apiPost.Topic is null
                ? null
                : references.Topics[apiPost.Topic.Id];

            ForumSection? section = apiPost.Forum is null
                ? null
                : references.Sections[apiPost.Forum.Id];

            ForumPost forumPost = existingPost ?? new ForumPost
            {
                ApiForumId = batch.ApiForumId,
                ExternalId = apiPost.Id,
                AuthorMid = mid
            };

            forumPost.PublishedAtUtc = apiPost.Date.UtcDateTime;
            forumPost.RawPoints = apiPost.Points;
            forumPost.Points = int.TryParse(
                apiPost.Points,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int points)
                ? points
                : null;
            forumPost.ContentHtml = apiPost.Content;
            forumPost.Topic = topic;
            forumPost.Section = section;
            forumPost.TopicId = topic?.Id;
            forumPost.SectionId = section?.Id;

            if (existingPost is null)
                _dbContext.ForumPosts.Add(forumPost);
        }

        HashSet<int> importedExternalIds = batch.PostsById.Keys.ToHashSet();
        List<ForumPost> postsInRange = await _dbContext.ForumPosts
            .Where(post =>
                post.ApiForumId == batch.ApiForumId &&
                post.AuthorMid == mid &&
                post.PublishedAtUtc >= dateRange.FromUtc &&
                post.PublishedAtUtc < dateRange.ToUtcExclusive)
            .ToListAsync(cancellationToken);

        foreach (ForumPost post in postsInRange)
        {
            if (!importedExternalIds.Contains(post.ExternalId))
                _dbContext.ForumPosts.Remove(post);
        }
    }
}
