using ForumWrapUp.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ForumWrapUp.Web.Security;

public static class UserAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddUserAuthentication(this IServiceCollection services, bool isDevelopment)
    {
        services.AddSingleton<LoginPasswordVerifier>();
        services.AddSingleton<LoginAttemptLimiter>();
        services.AddScoped<UserSignInService>();
        services.AddScoped<UserSessionEvents>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
            .AddCookie(options =>
            {
                options.Cookie.Name = "NillWrapUp.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = isDevelopment
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(15);
                options.SlidingExpiration = false;
                options.EventsType = typeof(UserSessionEvents);
            });

        return services;
    }
}
