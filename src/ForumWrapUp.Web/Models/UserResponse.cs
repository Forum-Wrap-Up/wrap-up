using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Web.Models;

public sealed class UserResponse
{
    public required int Mid { get; init; }

    public string? Nickname { get; init; }

    public string? AvatarUrl { get; init; }

    public DateTime? ForumRegisteredAtUtc { get; init; }

    public int? ForumMessageCount { get; init; }

    public bool? IsBanned { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required DateTime ProfileVerifiedAtUtc { get; init; }

    public DateTime? LastLoginAtUtc { get; init; }

    public required bool IsActive { get; init; }

    public static UserResponse FromAccount(UserAccount account)
    {
        return new UserResponse
        {
            Mid = account.Mid,
            Nickname = account.ForumProfile.Nickname,
            AvatarUrl = account.ForumProfile.AvatarUrl,
            ForumRegisteredAtUtc = account.ForumProfile.ForumRegisteredAtUtc,
            ForumMessageCount = account.ForumProfile.ForumMessageCount,
            IsBanned = account.ForumProfile.IsBanned,
            CreatedAtUtc = account.CreatedAtUtc,
            ProfileVerifiedAtUtc = account.ProfileVerifiedAtUtc,
            LastLoginAtUtc = account.LastLoginAtUtc,
            IsActive = account.IsActive
        };
    }
}
