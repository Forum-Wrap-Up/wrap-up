using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Web.Services;

public sealed class UserAccountService
{
    private readonly ForumWrapUpDbContext _dbContext;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public UserAccountService(ForumWrapUpDbContext dbContext, IPasswordHasher<UserAccount> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public Task<List<UserAccount>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.UserAccounts
            .AsNoTracking()
            .OrderBy(account => account.Mid)
            .ToListAsync(cancellationToken);
    }

    public Task<UserAccount?> GetByMidAsync(int mid, CancellationToken cancellationToken)
    {
        return _dbContext.UserAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account => account.Mid == mid, cancellationToken);
    }

    public async Task<UserAccount?> CreateAsync(int mid, string password, CancellationToken cancellationToken)
    {
        if (mid <= 0)
            throw new ArgumentOutOfRangeException(nameof(mid), "MID must be positive.");

        PasswordPolicyValidator.Validate(password);

        bool accountExists = await _dbContext.UserAccounts.AnyAsync(
            account => account.Mid == mid,
            cancellationToken);

        if (accountExists)
            return null;

        DateTime createdAtUtc = DateTime.UtcNow;
        UserAccount account = new()
        {
            Mid = mid,
            CreatedAtUtc = createdAtUtc,
            ProfileVerifiedAtUtc = createdAtUtc,
            IsActive = true
        };
        account.PasswordHash = _passwordHasher.HashPassword(account, password);

        _dbContext.UserAccounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<UserAccount?> SetPasswordAsync(int mid, string password, CancellationToken cancellationToken)
    {
        PasswordPolicyValidator.Validate(password);

        UserAccount? account = await _dbContext.UserAccounts
            .SingleOrDefaultAsync(item => item.Mid == mid, cancellationToken);

        if (account is null)
            return null;

        account.PasswordHash = _passwordHasher.HashPassword(account, password);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<bool> DeactivateAsync(int mid, CancellationToken cancellationToken)
    {
        UserAccount? account = await _dbContext.UserAccounts
            .SingleOrDefaultAsync(item => item.Mid == mid, cancellationToken);

        if (account is null)
            return false;

        if (!account.IsActive)
            return true;

        account.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ReactivateAsync(int mid, CancellationToken cancellationToken)
    {
        UserAccount? account = await _dbContext.UserAccounts
            .SingleOrDefaultAsync(item => item.Mid == mid, cancellationToken);

        if (account is null)
            return false;

        if (account.IsActive)
            return true;

        account.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
