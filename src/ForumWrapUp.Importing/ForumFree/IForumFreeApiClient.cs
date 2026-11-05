using ForumFree.Net.Models;

namespace ForumWrapUp.Importing.ForumFree;

public interface IForumFreeApiClient
{
    Task<ProfileResponse> GetProfileByIdAsync(int userId);

    Task<UserPostsResponse> GetPostsByUserIdAsync(int userId, int page);
}
