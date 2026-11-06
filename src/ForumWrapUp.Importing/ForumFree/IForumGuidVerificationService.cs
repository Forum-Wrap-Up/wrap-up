namespace ForumWrapUp.Importing.ForumFree;

public interface IForumGuidVerificationService
{
    Task<bool> VerifyAsync(int mid, Guid guid, CancellationToken cancellationToken = default);
}
