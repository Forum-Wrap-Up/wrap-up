using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Importing;

public sealed class ForumDataImportService : IForumDataImportService
{
    private readonly ForumImportOperationManager _operationManager;

    public ForumDataImportService(ForumImportOperationManager operationManager)
    {
        _operationManager = operationManager;
    }

    public async Task<ImportOperation> QueueUserDataImportAsync(int mid, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        if (mid <= 0)
            throw new ArgumentOutOfRangeException(nameof(mid), "MID must be positive.");

        ForumImportDateRange.Create(fromDate, toDate);
        return await _operationManager.CreateQueuedAsync(mid, fromDate, toDate, cancellationToken);
    }

    public Task<ImportOperation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken = default)
    {
        return _operationManager.GetOperationAsync(operationId, cancellationToken);
    }
}
