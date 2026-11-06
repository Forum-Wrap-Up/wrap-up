using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ForumWrapUp.Web.Tests;

public sealed class AccountLoginTests
{
    [Theory]
    [InlineData("/", null, 20000)]
    [InlineData("/Account/Login", null, 20000)]
    [InlineData("/", "00:02:00", 120000)]
    [InlineData("/Account/Login", "00:02:00", 120000)]
    [InlineData("/", "00:00:01.250", 1250)]
    [InlineData("/Account/Login", "00:00:01.250", 1250)]
    public async Task LoginFormUsesConfiguredOrDefaultTimeout(string path, string? configuredTimeout, int expectedTimeout)
    {
        await using var application = new LoginWebApplicationFactory();
        var settings = new Dictionary<string, string?>();

        if (configuredTimeout is not null)
            settings["LoginForm:ClientTimeout"] = configuredTimeout;

        await using WebApplicationFactory<Program> configuredApplication = application.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(settings);
            }));
        using HttpClient client = configuredApplication.CreateClient();

        string html = await client.GetStringAsync(path);

        Assert.Contains($"data-login-timeout-ms=\"{expectedTimeout}\"", html);
    }

    [Theory]
    [InlineData(null, 20000)]
    [InlineData("00:02:00", 120000)]
    [InlineData("00:00:01.250", 1250)]
    public async Task FailedLoginPreservesConfiguredOrDefaultTimeout(string? configuredTimeout, int expectedTimeout)
    {
        await using var application = new LoginWebApplicationFactory();
        var settings = new Dictionary<string, string?>();

        if (configuredTimeout is not null)
            settings["LoginForm:ClientTimeout"] = configuredTimeout;

        await using WebApplicationFactory<Program> configuredApplication = application.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(settings);
            }));
        var options = new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false };
        using HttpClient client = configuredApplication.CreateClient(options);

        using HttpResponseMessage response = await LoginAsync(client, "0", "password");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains($"data-login-timeout-ms=\"{expectedTimeout}\"", html);
    }

    [Fact]
    public async Task LoginShowsIdentityProtectsAdminApiAndLogoutEndsSession()
    {
        await using var application = new LoginWebApplicationFactory();
        await application.SeedAccountsAsync();
        using HttpClient client = application.CreateSessionClient();

        using HttpResponseMessage login = await LoginAsync(client, "123", LoginWebApplicationFactory.Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        IEnumerable<string> cookies = login.Headers.GetValues("Set-Cookie");
        string sessionCookie = Assert.Single(cookies, cookie => cookie.StartsWith("NillWrapUp.Session="));
        Assert.Contains("secure", sessionCookie);
        Assert.Contains("httponly", sessionCookie);
        Assert.Contains("samesite=lax", sessionCookie);

        string home = await client.GetStringAsync("/");
        Assert.Contains("account-menu", home);
        Assert.Contains("Il mio wrap-up", home);
        Assert.Contains(">Esci</button>", home);
        Assert.DoesNotContain("Ciao,", home);
        Assert.DoesNotContain("Nill Member", home);
        Assert.DoesNotContain("data-login-trigger", home);
        Assert.DoesNotContain(">Registrati</a>", home);
        Assert.Contains("no-store", login.Headers.CacheControl!.ToString());

        using HttpResponseMessage wrapUp = await client.GetAsync("/WrapUp");
        Assert.Equal(HttpStatusCode.OK, wrapUp.StatusCode);

        using HttpResponseMessage admin = await client.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, admin.StatusCode);

        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        UserAccount? account = await database.UserAccounts.FindAsync(123);
        Assert.NotNull(account!.LastLoginAtUtc);

        string token = ExtractToken(home);
        var fields = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        using var content = new FormUrlEncodedContent(fields);
        using HttpResponseMessage logout = await client.PostAsync("/Account/Logout", content);
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        using HttpResponseMessage protectedPage = await client.GetAsync("/WrapUp");
        Assert.Equal(HttpStatusCode.Redirect, protectedPage.StatusCode);
        Assert.StartsWith("https://localhost/Account/Login", protectedPage.Headers.Location!.ToString());

        string signedOutHome = await client.GetStringAsync("/");
        string decodedHome = WebUtility.HtmlDecode(signedOutHome);
        Assert.Contains("Hai effettuato l'uscita", decodedHome);
        Assert.Contains("data-login-trigger", signedOutHome);
    }

    [Theory]
    [InlineData("123", "password sbagliata")]
    [InlineData("999", LoginWebApplicationFactory.Password)]
    [InlineData("456", LoginWebApplicationFactory.Password)]
    public async Task WrongUnknownAndInactiveAccountsHaveTheSameError(string mid, string password)
    {
        await using var application = new LoginWebApplicationFactory();
        await application.SeedAccountsAsync();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage response = await LoginAsync(client, mid, password, dialog: true);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument result = JsonDocument.Parse(body);
        string? error = result.RootElement.GetProperty("errors").GetProperty("")[0].GetString();
        Assert.Equal("MID o password non corretti. Controlla i dati e riprova.", error);
        Assert.DoesNotContain(password, body);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out _));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("2147483648")]
    [InlineData("nickname")]
    public async Task InvalidMidReturnsAnItalianFieldError(string mid)
    {
        await using var application = new LoginWebApplicationFactory();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage response = await LoginAsync(client, mid, "password", dialog: true);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument result = JsonDocument.Parse(body);
        Assert.True(result.RootElement.GetProperty("errors").TryGetProperty("Mid", out _));
    }

    [Fact]
    public async Task NonJavascriptFailureKeepsTheFormButNeverEchoesThePassword()
    {
        await using var application = new LoginWebApplicationFactory();
        await application.SeedAccountsAsync();
        using HttpClient client = application.CreateSessionClient();
        const string password = "do not echo this password";
        using HttpResponseMessage response = await LoginAsync(client, "123", password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("MID o password non corretti.", body);
        Assert.Contains("value=\"123\"", body);
        Assert.DoesNotContain(password, body);
        Assert.DoesNotContain("<dialog", body);
    }

    [Theory]
    [InlineData("https://example.invalid", "/")]
    [InlineData("//example.invalid", "/")]
    [InlineData("/WrapUp", "/WrapUp")]
    public async Task LoginOnlyRedirectsToLocalDestinations(string returnUrl, string expected)
    {
        await using var application = new LoginWebApplicationFactory();
        await application.SeedAccountsAsync();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage response = await LoginAsync(
            client,
            "123",
            LoginWebApplicationFactory.Password,
            dialog: true,
            returnUrl: returnUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument result = JsonDocument.Parse(body);
        Assert.Equal(expected, result.RootElement.GetProperty("redirectUrl").GetString());
    }

    [Fact]
    public async Task MissingNicknameIsNotShownAndDeactivationRevokesTheSession()
    {
        await using var application = new LoginWebApplicationFactory();
        await application.SeedAccountsAsync();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage login = await LoginAsync(client, "789", LoginWebApplicationFactory.Password);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        string home = await client.GetStringAsync("/");
        Assert.Contains("Il mio wrap-up", home);
        Assert.Contains(">Esci</button>", home);
        Assert.DoesNotContain("Ciao,", home);
        Assert.DoesNotContain("MID 789", home);

        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        UserAccount? account = await database.UserAccounts.FindAsync(789);
        account!.IsActive = false;
        await database.SaveChangesAsync();

        using HttpResponseMessage response = await client.GetAsync("/WrapUp");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task LoginAttemptsAreLimitedWithoutClosingTheForm()
    {
        await using var application = new LoginWebApplicationFactory();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage page = await client.GetAsync("/Account/Login");
        string html = await page.Content.ReadAsStringAsync();
        string token = ExtractToken(html);

        for (int attempt = 0; attempt < 10; attempt++)
        {
            using HttpRequestMessage request = CreateLoginRequest(token, "999", "password", true, "/");
            using HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        using HttpRequestMessage rejectedRequest = CreateLoginRequest(token, "999", "password", false, "/");
        using HttpResponseMessage rejected = await client.SendAsync(rejectedRequest);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.Equal(TimeSpan.FromMinutes(1), rejected.Headers.RetryAfter!.Delta);
        string body = await rejected.Content.ReadAsStringAsync();
        Assert.Contains("Troppi tentativi", body);
        Assert.Contains("data-login-form", body);
    }

    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Logout")]
    public async Task AuthenticationPostsRequireAntiforgery(string path)
    {
        await using var application = new LoginWebApplicationFactory();
        using HttpClient client = application.CreateSessionClient();
        var fields = new Dictionary<string, string> { ["Mid"] = "123", ["Password"] = LoginWebApplicationFactory.Password };
        using var content = new FormUrlEncodedContent(fields);
        using HttpResponseMessage response = await client.PostAsync(path, content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PrivatePageRedirectsAnonymousVisitorsToLogin()
    {
        await using var application = new LoginWebApplicationFactory();
        using HttpClient client = application.CreateSessionClient();
        using HttpResponseMessage response = await client.GetAsync("/WrapUp");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("ReturnUrl=%2FWrapUp", response.Headers.Location!.ToString());
    }

    private static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string mid,
        string password,
        bool dialog = false,
        string returnUrl = "/")
    {
        using HttpResponseMessage page = await client.GetAsync("/Account/Login");
        string html = await page.Content.ReadAsStringAsync();
        string token = ExtractToken(html);
        using HttpRequestMessage request = CreateLoginRequest(token, mid, password, dialog, returnUrl);
        return await client.SendAsync(request);
    }

    private static HttpRequestMessage CreateLoginRequest(string token, string mid, string password, bool dialog, string returnUrl)
    {
        var fields = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Mid"] = mid,
            ["Password"] = password,
            ["ReturnUrl"] = returnUrl
        };
        var content = new FormUrlEncodedContent(fields);
        var request = new HttpRequestMessage(HttpMethod.Post, "/Account/Login") { Content = content };

        if (dialog)
            request.Headers.Add("X-Login-Dialog", "true");

        return request;
    }

    private static string ExtractToken(string html)
    {
        Match match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        string token = match.Groups[1].Value;
        return WebUtility.HtmlDecode(token);
    }
}
