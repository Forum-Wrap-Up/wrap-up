namespace ForumWrapUp.Data.Entities.Raw;

/// <summary>Forum sorgente identificato dal campo idForum delle risposte API.</summary>
public sealed class ForumInstance
{
    /// <summary>Identificativo remoto del forum nel suo complesso, distinto dagli ID delle sezioni.</summary>
    public int ApiForumId { get; set; }

    /// <summary>Sezioni del forum presenti nel database locale.</summary>
    public ICollection<ForumSection> Sections { get; } = [];

    /// <summary>Topic del forum presenti nel database locale.</summary>
    public ICollection<ForumTopic> Topics { get; } = [];

    /// <summary>Post importati da questo forum.</summary>
    public ICollection<ForumPost> Posts { get; } = [];
}
