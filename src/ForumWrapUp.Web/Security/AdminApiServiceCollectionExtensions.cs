using Microsoft.AspNetCore.Authentication;

namespace ForumWrapUp.Web.Security;

public static class AdminApiServiceCollectionExtensions
{
    public static IServiceCollection AddAdminApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminApiOptions>(configuration.GetSection(AdminApiOptions.SectionName));

        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, AdminApiAuthenticationHandler>(
                AdminApiDefaults.AuthenticationScheme,
                _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy(
                AdminApiDefaults.AuthorizationPolicy,
                policy => policy
                    .AddAuthenticationSchemes(AdminApiDefaults.AuthenticationScheme)
                    .RequireRole(AdminApiDefaults.AdministratorRole));

        return services;
    }
}
