using ApiPost = ForumFree.Net.Models.UserPost;

namespace ForumWrapUp.Importing;

public sealed record ForumPostImportBatch
{
    public int ApiForumId { get; }

    public IReadOnlyDictionary<int, ApiPost> PostsById { get; }

    public ForumPostImportBatch(int apiForumId, IReadOnlyDictionary<int, ApiPost> postsById)
    {
        ApiForumId = apiForumId;
        PostsById = postsById;
    }
}
