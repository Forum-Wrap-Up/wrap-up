using ForumFree.Net.Models;
using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Importing;

public sealed class ForumImportProcessor
{
    private readonly ForumProfileImporter _profileImporter;
    private readonly ForumPostPageImporter _pageImporter;
    private readonly ForumRawDataSynchronizer _rawDataSynchronizer;

    public ForumImportProcessor(ForumProfileImporter profileImporter, ForumPostPageImporter pageImporter, ForumRawDataSynchronizer rawDataSynchronizer)
    {
        _profileImporter = profileImporter;
        _pageImporter = pageImporter;
        _rawDataSynchronizer = rawDataSynchronizer;
    }

    public async Task ProcessAsync(ImportOperation operation, CancellationToken cancellationToken)
    {
        ForumImportDateRange dateRange = ForumImportDateRange.Create(operation.FromDate, operation.ToDate);
        UserProfile profile = await _profileImporter.GetByMidAsync(operation.Mid);
        ForumPostImportBatch posts = await _pageImporter.FetchAsync(operation, dateRange, cancellationToken);

        await _rawDataSynchronizer.SynchronizeAsync(operation, profile, posts, dateRange, cancellationToken);
    }
}
