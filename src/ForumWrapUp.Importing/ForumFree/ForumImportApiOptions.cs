namespace ForumWrapUp.Importing.ForumFree;

public sealed class ForumImportApiOptions
{
    public const string SectionName = "ForumApi";

    public Uri? ForumUri { get; set; }

    public TimeSpan MaximumPageDelay { get; set; }
}
