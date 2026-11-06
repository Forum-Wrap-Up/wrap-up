using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ForumWrapUp.Web.Security;

public sealed class UserSessionEvents : CookieAuthenticationEvents
{
    private readonly UserSignInService _signInService;

    public UserSessionEvents(UserSignInService signInService)
    {
        _signInService = signInService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        string? identifier = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        bool validMid = int.TryParse(identifier, NumberStyles.None, CultureInfo.InvariantCulture, out int mid);
        UserAccount? account = null;

        if (validMid)
        {
            try
            {
                account = await _signInService.FindActiveAccountAsync(mid, context.HttpContext.RequestAborted);
            }
            catch (DbException)
            {
                context.RejectPrincipal();
                return;
            }
        }

        if (account is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        string displayName = string.IsNullOrWhiteSpace(account.ForumProfile.Nickname)
            ? $"MID {account.Mid}"
            : account.ForumProfile.Nickname;
        var identity = new ClaimsIdentity(context.Principal!.Identity);
        Claim? previousName = identity.FindFirst(ClaimTypes.Name);

        if (previousName is not null)
            identity.RemoveClaim(previousName);

        var nameClaim = new Claim(ClaimTypes.Name, displayName);
        identity.AddClaim(nameClaim);
        var principal = new ClaimsPrincipal(identity);
        context.ReplacePrincipal(principal);
    }
}
