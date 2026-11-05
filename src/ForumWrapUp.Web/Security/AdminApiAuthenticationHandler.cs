using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Web.Security;

public sealed class AdminApiAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptionsMonitor<AdminApiOptions> _adminApiOptions;

    public AdminApiAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
        IOptionsMonitor<AdminApiOptions> adminApiOptions) : base(options, logger, encoder)
    {
        _adminApiOptions = adminApiOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue? header)
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(header.Parameter))
            return Task.FromResult(AuthenticateResult.Fail("Invalid admin API credentials."));

        string? configuredToken = _adminApiOptions.CurrentValue.Token;
        if (string.IsNullOrWhiteSpace(configuredToken))
            return Task.FromResult(AuthenticateResult.Fail("Admin API authentication is not configured."));

        byte[] expectedTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredToken));
        byte[] providedTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(header.Parameter));
        if (!CryptographicOperations.FixedTimeEquals(expectedTokenHash, providedTokenHash))
            return Task.FromResult(AuthenticateResult.Fail("Invalid admin API credentials."));

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, "admin-api"),
            new Claim(ClaimTypes.Role, AdminApiDefaults.AdministratorRole)
        ];
        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
