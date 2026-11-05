using ForumWrapUp.Data.Entities.Raw;

namespace ForumWrapUp.Importing;

public sealed record ForumRawReferences
{
    public IReadOnlyDictionary<int, ForumTopic> Topics { get; }

    public IReadOnlyDictionary<int, ForumSection> Sections { get; }

    public ForumRawReferences(IReadOnlyDictionary<int, ForumTopic> topics, IReadOnlyDictionary<int, ForumSection> sections)
    {
        Topics = topics;
        Sections = sections;
    }
}
