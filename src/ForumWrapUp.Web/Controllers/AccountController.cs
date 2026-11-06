using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Models;
using ForumWrapUp.Web.Security;
using ForumWrapUp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Web.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccountController : Controller
{
    private readonly UserSignInService _signInService;
    private readonly LoginAttemptLimiter _attemptLimiter;
    private readonly ILogger<AccountController> _logger;
    private readonly LoginFormOptions _loginFormOptions;

    public AccountController(UserSignInService signInService, LoginAttemptLimiter attemptLimiter, ILogger<AccountController> logger, IOptions<LoginFormOptions> loginFormOptions)
    {
        _signInService = signInService;
        _attemptLimiter = attemptLimiter;
        _logger = logger;
        _loginFormOptions = loginFormOptions.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        string destination = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";

        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(destination);

        var model = new LoginViewModel { ReturnUrl = destination, ClientTimeoutMs = (int)_loginFormOptions.ClientTimeout.TotalMilliseconds };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        using RateLimitLease lease = _attemptLimiter.Acquire(HttpContext);

        if (!lease.IsAcquired)
        {
            Response.Headers.RetryAfter = "60";
            ModelState.AddModelError(string.Empty, "Troppi tentativi di accesso. Attendi un minuto e riprova.");
            return LoginFailure(model, StatusCodes.Status429TooManyRequests);
        }

        bool validMid = int.TryParse(model.Mid, NumberStyles.None, CultureInfo.InvariantCulture, out int mid);

        if (ModelState.IsValid && (!validMid || mid <= 0))
            ModelState.AddModelError(nameof(model.Mid), "Inserisci un MID valido: un numero intero maggiore di zero.");

        if (!ModelState.IsValid)
            return LoginFailure(model, StatusCodes.Status400BadRequest);

        if (!_signInService.IsAvailable)
        {
            ModelState.AddModelError(string.Empty, "L'accesso non è disponibile al momento. Riprova più tardi.");
            return LoginFailure(model, StatusCodes.Status503ServiceUnavailable);
        }

        UserAccount? account;

        try
        {
            account = await _signInService.AuthenticateAsync(mid, model.Password, cancellationToken);
        }
        catch (DbException)
        {
            _logger.LogWarning("The account database is unavailable during sign-in.");
            ModelState.AddModelError(string.Empty, "Non riusciamo a completare l'accesso. Riprova tra poco.");
            return LoginFailure(model, StatusCodes.Status503ServiceUnavailable);
        }

        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "MID o password non corretti. Controlla i dati e riprova.");
            return LoginFailure(model, StatusCodes.Status401Unauthorized);
        }

        string identifier = account.Mid.ToString(CultureInfo.InvariantCulture);
        string displayName = string.IsNullOrWhiteSpace(account.ForumProfile.Nickname)
            ? $"MID {identifier}"
            : account.ForumProfile.Nickname;
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, identifier),
            new Claim(ClaimTypes.Name, displayName)
        ];
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties { IsPersistent = true, AllowRefresh = false };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        string destination = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/";

        if (Request.Headers["X-Login-Dialog"] == "true")
            return Json(new { redirectUrl = destination });

        return LocalRedirect(destination);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["StatusMessage"] = "Hai effettuato l'uscita dal tuo account.";
        return RedirectToAction("Index", "Home");
    }

    private IActionResult LoginFailure(LoginViewModel model, int statusCode)
    {
        Response.StatusCode = statusCode;
        model.Password = string.Empty;
        model.ReturnUrl = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : "/";
        model.ClientTimeoutMs = (int)_loginFormOptions.ClientTimeout.TotalMilliseconds;

        if (Request.Headers["X-Login-Dialog"] == "true")
        {
            Dictionary<string, string[]> errors = ModelState
                .Where(entry => entry.Value!.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());
            return Json(new { errors });
        }

        return View("Login", model);
    }
}
