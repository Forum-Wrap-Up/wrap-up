namespace ForumWrapUp.Data.Entities.Raw;

public sealed class ForumProfile
{
    public int Mid { get; set; }

    public string? Nickname { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? ForumRegisteredAtUtc { get; set; }

    public int? ForumMessageCount { get; set; }

    public bool? IsBanned { get; set; }

    public DateTime? LastSyncedAtUtc { get; set; }

    public UserAccount? UserAccount { get; set; }
}
