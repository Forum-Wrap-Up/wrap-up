using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Data.Entities.Raw;
using ForumWrapUp.Importing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ForumWrapUp.Web.Tests;

public sealed class LoginWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string Password = "una passphrase di prova";
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:ForumWrapUp", string.Empty);
        builder.ConfigureTestServices(services =>
        {
            ServiceDescriptor[] workers = services
                .Where(service => service.ServiceType == typeof(IHostedService)
                    && service.ImplementationType == typeof(ForumImportBackgroundService))
                .ToArray();

            foreach (ServiceDescriptor worker in workers)
                services.Remove(worker);

            services.RemoveAll<ForumWrapUpDbContext>();
            services.RemoveAll<DbContextOptions<ForumWrapUpDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ForumWrapUpDbContext>>();
            services.AddDbContext<ForumWrapUpDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task SeedAccountsAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        IPasswordHasher<UserAccount> hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserAccount>>();

        UserAccount[] accounts =
        [
            new UserAccount { Mid = 123, ForumProfile = new ForumProfile { Mid = 123, Nickname = "Nill Member" } },
            new UserAccount { Mid = 456, ForumProfile = new ForumProfile { Mid = 456 }, IsActive = false },
            new UserAccount { Mid = 789, ForumProfile = new ForumProfile { Mid = 789 } }
        ];

        foreach (UserAccount account in accounts)
            account.PasswordHash = hasher.HashPassword(account, Password);

        database.UserAccounts.AddRange(accounts);
        await database.SaveChangesAsync();
    }

    public HttpClient CreateSessionClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }
}
