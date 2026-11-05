using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ForumWrapUp.Data;

public static class ForumWrapUpDataServiceCollectionExtensions
{
    public static IServiceCollection AddForumWrapUpData(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ForumWrapUpDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
