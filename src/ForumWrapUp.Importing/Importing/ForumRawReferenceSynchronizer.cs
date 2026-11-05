using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities.Raw;
using Microsoft.EntityFrameworkCore;
using ApiPost = ForumFree.Net.Models.UserPost;

namespace ForumWrapUp.Importing;

public sealed class ForumRawReferenceSynchronizer
{
    private readonly ForumWrapUpDbContext _dbContext;

    public ForumRawReferenceSynchronizer(ForumWrapUpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ForumRawReferences> SynchronizeAsync(int apiForumId, IReadOnlyDictionary<int, ApiPost> postsById, CancellationToken cancellationToken)
    {
        int[] topicExternalIds = postsById.Values
            .Where(post => post.Topic is not null)
            .Select(post => post.Topic!.Id)
            .Distinct()
            .ToArray();

        int[] sectionExternalIds = postsById.Values
            .Where(post => post.Forum is not null)
            .Select(post => post.Forum!.Id)
            .Distinct()
            .ToArray();

        Dictionary<int, ForumTopic> topics = topicExternalIds.Length == 0
            ? []
            : await _dbContext.ForumTopics
                .Where(topic => topic.ApiForumId == apiForumId && topicExternalIds.Contains(topic.ExternalId))
                .ToDictionaryAsync(topic => topic.ExternalId, cancellationToken);

        Dictionary<int, ForumSection> sections = sectionExternalIds.Length == 0
            ? []
            : await _dbContext.ForumSections
                .Where(section => section.ApiForumId == apiForumId && sectionExternalIds.Contains(section.ExternalId))
                .ToDictionaryAsync(section => section.ExternalId, cancellationToken);

        foreach (ApiPost post in postsById.Values)
        {
            if (post.Topic is not null)
            {
                if (!topics.TryGetValue(post.Topic.Id, out ForumTopic? topic))
                {
                    topic = new ForumTopic
                    {
                        ApiForumId = apiForumId,
                        ExternalId = post.Topic.Id
                    };
                    _dbContext.ForumTopics.Add(topic);
                    topics.Add(post.Topic.Id, topic);
                }

                topic.TitleHtml = post.Topic.Title;
                topic.PostCount = post.Topic.Posts;
            }

            if (post.Forum is not null)
            {
                if (!sections.TryGetValue(post.Forum.Id, out ForumSection? section))
                {
                    section = new ForumSection
                    {
                        ApiForumId = apiForumId,
                        ExternalId = post.Forum.Id
                    };
                    _dbContext.ForumSections.Add(section);
                    sections.Add(post.Forum.Id, section);
                }

                section.Name = post.Forum.Name;
            }
        }

        return new ForumRawReferences(topics, sections);
    }
}
