using ForumFree.Net.Models;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForumWrapUp.Importing;

public sealed class ForumImportOperationManager
{
    private readonly ForumWrapUpDbContext _dbContext;
    private readonly ILogger<ForumImportOperationManager> _logger;

    public ForumImportOperationManager(ForumWrapUpDbContext dbContext, ILogger<ForumImportOperationManager> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportOperation> CreateQueuedAsync(int mid, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        bool userExists = await _dbContext.UserAccounts.AnyAsync(
            account => account.Mid == mid && account.IsActive,
            cancellationToken);

        if (!userExists)
            throw new KeyNotFoundException($"No active local account exists for MID {mid}.");

        ImportOperation operation = new()
        {
            Id = Guid.NewGuid(),
            Mid = mid,
            FromDate = fromDate,
            ToDate = toDate,
            Status = ImportOperationStatus.Queued,
            RequestedAtUtc = DateTime.UtcNow
        };

        _dbContext.ImportOperations.Add(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return operation;
    }

    public async Task<ImportOperation?> TryStartNextQueuedAsync(CancellationToken cancellationToken)
    {
        Guid? queuedOperationId = await _dbContext.ImportOperations
            .Where(operation => operation.Status == ImportOperationStatus.Queued)
            .OrderBy(operation => operation.RequestedAtUtc)
            .ThenBy(operation => operation.Id)
            .Select(operation => (Guid?)operation.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!queuedOperationId.HasValue)
            return null;

        DateTime startedAtUtc = DateTime.UtcNow;
        int updatedOperations = await _dbContext.ImportOperations
            .Where(operation => operation.Id == queuedOperationId.Value && operation.Status == ImportOperationStatus.Queued)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(operation => operation.Status, ImportOperationStatus.Running)
                    .SetProperty(operation => operation.StartedAtUtc, startedAtUtc),
                cancellationToken);

        if (updatedOperations == 0)
            return null;

        return await _dbContext.ImportOperations
            .SingleAsync(operation => operation.Id == queuedOperationId.Value, cancellationToken);
    }

    public Task RequeueInterruptedOperationsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.ImportOperations
            .Where(operation => operation.Status == ImportOperationStatus.Running)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(operation => operation.Status, ImportOperationStatus.Queued)
                    .SetProperty(operation => operation.StartedAtUtc, (DateTime?)null)
                    .SetProperty(operation => operation.PagesFetched, 0)
                    .SetProperty(operation => operation.TotalPagesExpected, (int?)null)
                    .SetProperty(operation => operation.PostsFetched, 0)
                    .SetProperty(operation => operation.TotalPostsExpected, (int?)null)
                    .SetProperty(operation => operation.ErrorMessage, (string?)null),
                cancellationToken);
    }

    public Task<ImportOperation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken)
    {
        return _dbContext.ImportOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(operation => operation.Id == operationId, cancellationToken);
    }

    public async Task ReportPageFetchedAsync(ImportOperation operation,UserPostsResponse response,CancellationToken cancellationToken)
    {
        if (operation.PagesFetched == 0)
        {
            operation.TotalPagesExpected = response.Pages;
            operation.TotalPostsExpected = response.Total;
        }

        operation.PagesFetched++;
        operation.PostsFetched += response.Posts.Count;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task MarkCancelledAsync(Guid operationId)
    {
        return MarkTerminalAsync(operationId, ImportOperationStatus.Cancelled, null);
    }

    public Task MarkFailedAsync(Guid operationId, Exception exception)
    {
        string exceptionType = exception.GetType().Name;
        _logger.LogError(
            "Import operation {ImportOperationId} failed with {ExceptionType}.",
            operationId,
            exceptionType);

        string errorMessage = $"Import failed ({exceptionType}).";

        return MarkTerminalAsync(operationId, ImportOperationStatus.Failed, errorMessage);
    }

    private async Task MarkTerminalAsync(Guid operationId, ImportOperationStatus status, string? errorMessage)
    {
        _dbContext.ChangeTracker.Clear();
        ImportOperation operation = await _dbContext.ImportOperations.SingleAsync(item => item.Id == operationId, CancellationToken.None);
        operation.Status = status;
        operation.CompletedAtUtc = DateTime.UtcNow;
        operation.ErrorMessage = errorMessage;

        await _dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
