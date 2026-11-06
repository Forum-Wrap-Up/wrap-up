using System.Security.Claims;
using ForumWrapUp.Web.Models;
using ForumWrapUp.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumWrapUp.Web.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class WrapUpController : Controller
{
    private readonly WrapUpYearService _wrapUpYearService;

    public WrapUpController(WrapUpYearService wrapUpYearService)
    {
        _wrapUpYearService = wrapUpYearService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        string? identifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(identifier, out int mid))
            return Unauthorized();

        List<WrapUpYearAvailability> years = await _wrapUpYearService.GetEnabledAvailabilityAsync(mid, cancellationToken);
        return View(years);
    }
}
