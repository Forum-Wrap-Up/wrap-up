namespace ForumWrapUp.Web.Validation;

public static class PasswordPolicyValidator
{
    public const int MinimumLength = 12;

    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumLength)
            throw new ArgumentException($"Password must be at least {MinimumLength} characters.", nameof(password));
    }
}
