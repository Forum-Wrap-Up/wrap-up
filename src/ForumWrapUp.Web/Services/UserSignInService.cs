using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Web.Services;

public sealed class UserSignInService
{
    private readonly LoginPasswordVerifier _passwordVerifier;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;
    private readonly ForumWrapUpDbContext? _dbContext;

    public UserSignInService(LoginPasswordVerifier passwordVerifier, IPasswordHasher<UserAccount> passwordHasher, ForumWrapUpDbContext? dbContext = null)
    {
        _passwordVerifier = passwordVerifier;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
    }

    public bool IsAvailable => _dbContext is not null;

    public async Task<UserAccount?> AuthenticateAsync(int mid, string password, CancellationToken cancellationToken)
    {
        if (_dbContext is null)
            return null;

        UserAccount? account = await _dbContext.UserAccounts
            .Include(item => item.ForumProfile)
            .SingleOrDefaultAsync(item => item.Mid == mid, cancellationToken);

        PasswordVerificationResult result = _passwordVerifier.Verify(account, password);

        if (account is null || !account.IsActive || result == PasswordVerificationResult.Failed)
            return null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            account.PasswordHash = _passwordHasher.HashPassword(account, password);

        account.LastLoginAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<UserAccount?> FindActiveAccountAsync(int mid, CancellationToken cancellationToken)
    {
        if (_dbContext is null)
            return null;

        return await _dbContext.UserAccounts
            .AsNoTracking()
            .Include(account => account.ForumProfile)
            .SingleOrDefaultAsync(account => account.Mid == mid && account.IsActive, cancellationToken);
    }
}
