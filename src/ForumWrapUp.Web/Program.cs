using ForumWrapUp.Data;
using ForumWrapUp.Importing;
using ForumWrapUp.Importing.ForumFree;
using ForumWrapUp.Web.ForumFree;
using ForumWrapUp.Web.Security;

namespace ForumWrapUp.Web;

public sealed class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddForumWrapUpImporting(builder.Configuration);
        builder.Services.AddSingleton<IForumApiCookieProvider, ConfigurationForumApiCookieProvider>();
        builder.Services.AddAdminApiAuthentication(builder.Configuration);

        string? connectionString = builder.Configuration.GetConnectionString("ForumWrapUp");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.Services.AddForumWrapUpData(connectionString);
            builder.Services.AddHostedService<ForumImportBackgroundService>();
        }

        WebApplication app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();

        if (!string.IsNullOrWhiteSpace(connectionString))
            app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
