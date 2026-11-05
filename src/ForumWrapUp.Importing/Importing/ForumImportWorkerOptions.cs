namespace ForumWrapUp.Importing;

public sealed class ForumImportWorkerOptions
{
    public const string SectionName = "ForumImport";

    public TimeSpan QueueCheckInterval { get; set; } = TimeSpan.FromMinutes(15);
}
