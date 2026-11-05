using ForumFree.Net.Models;
using ForumWrapUp.Importing.ForumFree;

namespace ForumWrapUp.Importing;

public sealed class ForumProfileImporter
{
    private readonly IForumFreeApiClient _forumApiClient;

    public ForumProfileImporter(IForumFreeApiClient forumApiClient)
    {
        _forumApiClient = forumApiClient;
    }

    public async Task<UserProfile> GetByMidAsync(int mid)
    {
        ProfileResponse response = await _forumApiClient.GetProfileByIdAsync(mid);
        UserProfile? profile = response.Profiles.Values.SingleOrDefault(candidate => candidate.Id == mid);

        return profile ?? throw new InvalidDataException("The forum API did not return the requested profile.");
    }
}
