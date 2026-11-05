using ForumFree.Net;
using ForumFree.Net.Models;

namespace ForumWrapUp.Importing.ForumFree;

public sealed class ForumFreeApiClient : IForumFreeApiClient
{
    private readonly ForumFreeClient _forumFreeClient;

    public ForumFreeApiClient(ForumFreeClient forumFreeClient)
    {
        _forumFreeClient = forumFreeClient;
    }

    public Task<ProfileResponse> GetProfileByIdAsync(int userId)
    {
        return _forumFreeClient.GetProfileByIdAsync(userId);
    }

    public Task<UserPostsResponse> GetPostsByUserIdAsync(int userId, int page)
    {
        return _forumFreeClient.GetPostsByUserIdAsync(userId, page);
    }
}
