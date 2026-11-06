using ForumFree.Net;
using ForumWrapUp.Importing.ForumFree;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Importing;

public static class ForumImportServiceCollectionExtensions
{
    public static IServiceCollection AddForumWrapUpImporting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForumImportApiOptions>(configuration.GetSection(ForumImportApiOptions.SectionName));
        services.Configure<ForumImportWorkerOptions>(configuration.GetSection(ForumImportWorkerOptions.SectionName));

        services.AddTransient<ForumFreeCookieHandler>();
        services.AddHttpClient<ForumFreeClient>(ConfigureForumApiClient)
            .AddHttpMessageHandler<ForumFreeCookieHandler>();

        services.AddHttpClient<IForumGuidVerificationService, ForumGuidVerificationService>(ConfigureForumApiClient)
            .AddHttpMessageHandler<ForumFreeCookieHandler>();

        services.AddScoped<IForumFreeApiClient, ForumFreeApiClient>();
        services.AddScoped<ForumImportOperationManager>();
        services.AddScoped<ForumImportProcessor>();
        services.AddScoped<ForumProfileService>();
        services.AddScoped<ForumProfileImporter>();
        services.AddScoped<ForumPostPageImporter>();
        services.AddScoped<ForumRawReferenceSynchronizer>();
        services.AddScoped<ForumRawPostSynchronizer>();
        services.AddScoped<ForumRawDataSynchronizer>();
        services.AddScoped<IForumDataImportService, ForumDataImportService>();

        return services;
    }

    private static void ConfigureForumApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        ForumImportApiOptions options = serviceProvider.GetRequiredService<IOptions<ForumImportApiOptions>>().Value;

        if (options.ForumUri is null || !options.ForumUri.IsAbsoluteUri || options.ForumUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("ForumApi:ForumUri must be configured as an absolute HTTPS URI.");

        client.BaseAddress = options.ForumUri;
    }
}
