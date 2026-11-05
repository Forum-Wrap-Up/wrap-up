using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Importing;

public sealed class ForumImportProcessor
{
    private readonly ForumProfileService _profileService;
    private readonly ForumPostPageImporter _pageImporter;
    private readonly ForumRawDataSynchronizer _rawDataSynchronizer;

    public ForumImportProcessor(ForumProfileService profileService, ForumPostPageImporter pageImporter, ForumRawDataSynchronizer rawDataSynchronizer)
    {
        _profileService = profileService;
        _pageImporter = pageImporter;
        _rawDataSynchronizer = rawDataSynchronizer;
    }

    public async Task ProcessAsync(ImportOperation operation, CancellationToken cancellationToken)
    {
        ForumImportDateRange dateRange = ForumImportDateRange.Create(operation.FromDate, operation.ToDate);
        await _profileService.RefreshIfUninitializedAsync(operation.Mid, cancellationToken);
        ForumPostImportBatch posts = await _pageImporter.FetchAsync(operation, dateRange, cancellationToken);

        await _rawDataSynchronizer.SynchronizeAsync(operation, posts, dateRange, cancellationToken);
    }
}
