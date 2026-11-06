using ForumWrapUp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Web.ViewComponents;

public sealed class LoginFormViewComponent : ViewComponent
{
    private readonly LoginFormOptions _options;

    public LoginFormViewComponent(IOptions<LoginFormOptions> options)
    {
        _options = options.Value;
    }

    public IViewComponentResult Invoke()
    {
        var model = new LoginViewModel { ClientTimeoutMs = (int)_options.ClientTimeout.TotalMilliseconds };
        return View("~/Views/Shared/_LoginForm.cshtml", model);
    }
}
