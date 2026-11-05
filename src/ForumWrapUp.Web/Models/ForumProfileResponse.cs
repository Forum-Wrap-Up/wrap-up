using ForumWrapUp.Data.Entities.Raw;

namespace ForumWrapUp.Web.Models;

public sealed class ForumProfileResponse
{
    public required int Mid { get; init; }

    public string? Nickname { get; init; }

    public string? AvatarUrl { get; init; }

    public DateTime? ForumRegisteredAtUtc { get; init; }

    public int? ForumMessageCount { get; init; }

    public bool? IsBanned { get; init; }

    public required DateTime LastSyncedAtUtc { get; init; }

    public static ForumProfileResponse FromProfile(ForumProfile profile)
    {
        return new ForumProfileResponse
        {
            Mid = profile.Mid,
            Nickname = profile.Nickname,
            AvatarUrl = profile.AvatarUrl,
            ForumRegisteredAtUtc = profile.ForumRegisteredAtUtc,
            ForumMessageCount = profile.ForumMessageCount,
            IsBanned = profile.IsBanned,
            LastSyncedAtUtc = profile.LastSyncedAtUtc
                ?? throw new InvalidOperationException("The ForumFree profile has not been synchronized.")
        };
    }
}
