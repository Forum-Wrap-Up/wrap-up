using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ForumWrapUp.Web.Tests;

public sealed class WrapUpYearTests
{
    private const string AdminToken = "test-admin-token";

    [Fact]
    public async Task IndexShowsEnabledYearsAndOnlyTheCurrentUsersReadyWrapUps()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedWrapUpYearsAsync(application);

        using HttpClient client = application.CreateSessionClient();
        await SignInAsync(client, "123");

        using HttpResponseMessage response = await client.GetAsync("/WrapUp");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("data-year=\"2025\" data-available=\"true\"", html);
        Assert.Contains("data-year=\"2026\" data-available=\"false\"", html);
        Assert.Contains("data-year=\"2028\" data-available=\"false\"", html);
        Assert.Contains("data-year=\"2029\" data-available=\"false\"", html);
        Assert.DoesNotContain("data-year=\"2027\"", html);
    }

    [Fact]
    public async Task IndexExplainsWhenNoYearsAreEnabled()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        database.WrapUpYears.Add(new WrapUpYear { Year = 2027, IsEnabled = false, CreatedAtUtc = DateTime.UtcNow });
        await database.SaveChangesAsync();

        using HttpClient client = application.CreateSessionClient();
        await SignInAsync(client, "123");

        string html = await client.GetStringAsync("/WrapUp");

        Assert.Contains("Non ci sono ancora anni disponibili.", html);
        Assert.DoesNotContain("data-year=", html);
    }

    [Fact]
    public async Task AdminApiCanConfigureYearsAndRejectsUserSessions()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();

        await using WebApplicationFactory<Program> adminApplication = application.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                Dictionary<string, string?> settings = new()
                {
                    ["AdminApi:Token"] = AdminToken
                };
                configuration.AddInMemoryCollection(settings);
            }));

        using HttpClient client = adminApplication.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        using HttpResponseMessage anonymousResponse = await client.GetAsync("/api/admin/wrap-up-years");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        await SignInAsync(client, "123");
        using HttpResponseMessage userResponse = await client.GetAsync("/api/admin/wrap-up-years");
        Assert.Equal(HttpStatusCode.Unauthorized, userResponse.StatusCode);

        using HttpRequestMessage invalidYearRequest = CreateAdminRequest(HttpMethod.Put, "/api/admin/wrap-up-years/10000");
        invalidYearRequest.Content = JsonContent.Create(new UpdateWrapUpYearRequest { IsEnabled = true });
        using HttpResponseMessage invalidYearResponse = await client.SendAsync(invalidYearRequest);
        Assert.Equal(HttpStatusCode.BadRequest, invalidYearResponse.StatusCode);

        using HttpRequestMessage missingEnabledRequest = CreateAdminRequest(HttpMethod.Put, "/api/admin/wrap-up-years/2026");
        missingEnabledRequest.Content = JsonContent.Create(new { });
        using HttpResponseMessage missingEnabledResponse = await client.SendAsync(missingEnabledRequest);
        Assert.Equal(HttpStatusCode.BadRequest, missingEnabledResponse.StatusCode);

        using HttpRequestMessage enableRequest = CreateAdminRequest(HttpMethod.Put, "/api/admin/wrap-up-years/2026");
        enableRequest.Content = JsonContent.Create(new UpdateWrapUpYearRequest { IsEnabled = true });
        using HttpResponseMessage enableResponse = await client.SendAsync(enableRequest);
        WrapUpYearResponse? enabledYear = await enableResponse.Content.ReadFromJsonAsync<WrapUpYearResponse>();

        Assert.Equal(HttpStatusCode.OK, enableResponse.StatusCode);
        Assert.NotNull(enabledYear);
        Assert.Equal(2026, enabledYear.Year);
        Assert.True(enabledYear.IsEnabled);

        using HttpRequestMessage disableRequest = CreateAdminRequest(HttpMethod.Put, "/api/admin/wrap-up-years/2026");
        disableRequest.Content = JsonContent.Create(new UpdateWrapUpYearRequest { IsEnabled = false });
        using HttpResponseMessage disableResponse = await client.SendAsync(disableRequest);

        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);
        WrapUpYearResponse? disabledYear = await disableResponse.Content.ReadFromJsonAsync<WrapUpYearResponse>();
        Assert.NotNull(disabledYear);
        Assert.False(disabledYear.IsEnabled);

        using HttpRequestMessage listRequest = CreateAdminRequest(HttpMethod.Get, "/api/admin/wrap-up-years");
        using HttpResponseMessage listResponse = await client.SendAsync(listRequest);
        List<WrapUpYearResponse>? years = await listResponse.Content.ReadFromJsonAsync<List<WrapUpYearResponse>>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        WrapUpYearResponse configuredYear = Assert.Single(years!, year => year.Year == 2026);
        Assert.False(configuredYear.IsEnabled);
    }

    private static async Task SeedWrapUpYearsAsync(LoginWebApplicationFactory application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();

        database.WrapUpYears.AddRange(
            new WrapUpYear { Year = 2025, IsEnabled = true, CreatedAtUtc = DateTime.UtcNow },
            new WrapUpYear { Year = 2026, IsEnabled = true, CreatedAtUtc = DateTime.UtcNow },
            new WrapUpYear { Year = 2027, IsEnabled = false, CreatedAtUtc = DateTime.UtcNow },
            new WrapUpYear { Year = 2028, IsEnabled = true, CreatedAtUtc = DateTime.UtcNow },
            new WrapUpYear { Year = 2029, IsEnabled = true, CreatedAtUtc = DateTime.UtcNow });
        database.AnnualWrapUps.AddRange(
            new AnnualWrapUp { Mid = 123, Year = 2025, IsReady = true },
            new AnnualWrapUp { Mid = 123, Year = 2026, IsReady = false },
            new AnnualWrapUp { Mid = 123, Year = 2027, IsReady = true },
            new AnnualWrapUp { Mid = 789, Year = 2028, IsReady = true },
            new AnnualWrapUp { Mid = 123, Year = 2029, IsReady = false });

        await database.SaveChangesAsync();
    }

    private static async Task SignInAsync(HttpClient client, string mid)
    {
        using HttpResponseMessage page = await client.GetAsync("/Account/Login");
        string html = await page.Content.ReadAsStringAsync();
        Match match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        string token = WebUtility.HtmlDecode(match.Groups[1].Value);
        Dictionary<string, string> fields = new()
        {
            ["__RequestVerificationToken"] = token,
            ["Mid"] = mid,
            ["Password"] = LoginWebApplicationFactory.Password,
            ["ReturnUrl"] = "/"
        };
        using FormUrlEncodedContent content = new(fields);
        using HttpResponseMessage response = await client.PostAsync("/Account/Login", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static HttpRequestMessage CreateAdminRequest(HttpMethod method, string uri)
    {
        HttpRequestMessage request = new(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
        return request;
    }
}
