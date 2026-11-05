using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Importing;

public interface IForumDataImportService
{
    Task<ImportOperation> QueueUserDataImportAsync(int mid, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<ImportOperation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
}
