using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Data.Entities.Raw;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Importing;

public sealed class ForumRawDataSynchronizer
{
    private readonly ForumWrapUpDbContext _dbContext;
    private readonly ForumRawReferenceSynchronizer _referenceSynchronizer;
    private readonly ForumRawPostSynchronizer _postSynchronizer;

    public ForumRawDataSynchronizer(ForumWrapUpDbContext dbContext, ForumRawReferenceSynchronizer referenceSynchronizer, ForumRawPostSynchronizer postSynchronizer)
    {
        _dbContext = dbContext;
        _referenceSynchronizer = referenceSynchronizer;
        _postSynchronizer = postSynchronizer;
    }

    public async Task SynchronizeAsync(ImportOperation operation, ForumPostImportBatch batch, ForumImportDateRange dateRange, CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        UserAccount? user = await _dbContext.UserAccounts
            .SingleOrDefaultAsync(account => account.Mid == operation.Mid, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"No local account exists for MID {operation.Mid}.");

        ForumInstance? forum = await _dbContext.ForumInstances
            .SingleOrDefaultAsync(item => item.ApiForumId == batch.ApiForumId, cancellationToken);

        if (forum is null)
            _dbContext.ForumInstances.Add(new ForumInstance { ApiForumId = batch.ApiForumId });

        ForumRawReferences references = await _referenceSynchronizer.SynchronizeAsync(batch.ApiForumId, batch.PostsById, cancellationToken);

        await _postSynchronizer.SynchronizeAsync(operation.Mid, batch, references, dateRange, cancellationToken);

        operation.Status = ImportOperationStatus.Succeeded;
        operation.CompletedAtUtc = DateTime.UtcNow;
        operation.ErrorMessage = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
