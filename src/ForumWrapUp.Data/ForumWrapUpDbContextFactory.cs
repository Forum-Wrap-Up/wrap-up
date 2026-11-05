using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForumWrapUp.Data;

public sealed class ForumWrapUpDbContextFactory : IDesignTimeDbContextFactory<ForumWrapUpDbContext>
{
    public ForumWrapUpDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("FORUM_WRAPUP_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__ForumWrapUp")
            ?? "Host=localhost;Database=forum_wrapup;Username=postgres";

        DbContextOptions<ForumWrapUpDbContext> options = new DbContextOptionsBuilder<ForumWrapUpDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ForumWrapUpDbContext(options);
    }
}
