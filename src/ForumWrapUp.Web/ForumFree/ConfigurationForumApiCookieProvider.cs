using ForumWrapUp.Importing.ForumFree;
using Microsoft.Extensions.Configuration;

namespace ForumWrapUp.Web.ForumFree;

public sealed class ConfigurationForumApiCookieProvider : IForumApiCookieProvider
{
    private const string CookieConfigurationKey = "ForumApi:Cookie";

    private readonly IConfiguration _configuration;

    public ConfigurationForumApiCookieProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ValueTask<string> GetCookieAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? cookie = _configuration[CookieConfigurationKey];
        if (string.IsNullOrWhiteSpace(cookie))
            throw new InvalidOperationException($"{CookieConfigurationKey} is not configured. Set it in .NET User Secrets or another secure configuration provider.");

        return ValueTask.FromResult(cookie);
    }
}
