namespace ForumWrapUp.Web.Models;

public sealed class AnnualWrapUpGenerationOptions
{
    public const string SectionName = "AnnualWrapUpGeneration";

    public bool RequireFullYearImportCoverage { get; set; } = true;
}
