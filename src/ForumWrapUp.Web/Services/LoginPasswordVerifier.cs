using ForumWrapUp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace ForumWrapUp.Web.Services;

public sealed class LoginPasswordVerifier
{
    private readonly IPasswordHasher<UserAccount> _passwordHasher;
    private readonly UserAccount _unknownAccount;

    public LoginPasswordVerifier(IPasswordHasher<UserAccount> passwordHasher)
    {
        _passwordHasher = passwordHasher;
        _unknownAccount = new UserAccount();
        string dummyPassword = Guid.NewGuid().ToString();
        _unknownAccount.PasswordHash = _passwordHasher.HashPassword(_unknownAccount, dummyPassword);
    }

    public PasswordVerificationResult Verify(UserAccount? account, string password)
    {
        UserAccount candidate = account ?? _unknownAccount;
        return _passwordHasher.VerifyHashedPassword(candidate, candidate.PasswordHash, password);
    }
}
