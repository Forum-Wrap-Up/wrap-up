using ForumFree.Net.Models;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities.Raw;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Importing;

public sealed class ForumProfileService
{
    private readonly ForumWrapUpDbContext _dbContext;
    private readonly ForumProfileImporter _profileImporter;

    public ForumProfileService(ForumWrapUpDbContext dbContext, ForumProfileImporter profileImporter)
    {
        _dbContext = dbContext;
        _profileImporter = profileImporter;
    }

    public async Task<ForumProfile> AddOrRefreshAsync(int mid, CancellationToken cancellationToken)
    {
        UserProfile profile = await _profileImporter.GetByMidAsync(mid);
        ForumProfile? forumProfile = await _dbContext.ForumProfiles
            .SingleOrDefaultAsync(item => item.Mid == mid, cancellationToken);

        if (forumProfile is null)
        {
            forumProfile = new ForumProfile { Mid = mid };
            _dbContext.ForumProfiles.Add(forumProfile);
        }

        forumProfile.Nickname = profile.Nickname;
        forumProfile.AvatarUrl = profile.Avatar;
        forumProfile.ForumRegisteredAtUtc = profile.Registration.UtcDateTime;
        forumProfile.ForumMessageCount = profile.Messages;
        forumProfile.IsBanned = profile.Banned != 0;
        forumProfile.LastSyncedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return forumProfile;
    }

    public async Task<bool> RefreshIfUninitializedAsync(int mid, CancellationToken cancellationToken)
    {
        bool profileInitialized = await _dbContext.ForumProfiles
            .AnyAsync(
                profile => profile.Mid == mid && profile.LastSyncedAtUtc != null,
                cancellationToken);

        if (profileInitialized)
            return false;

        await AddOrRefreshAsync(mid, cancellationToken);
        return true;
    }
}
