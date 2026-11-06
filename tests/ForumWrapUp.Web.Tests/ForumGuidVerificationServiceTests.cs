using ForumWrapUp.Importing.ForumFree;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace ForumWrapUp.Web.Tests;

public sealed class ForumGuidVerificationServiceTests
{
    [Fact]
    public async Task VerifyAsync_WhenGuidIsInProfileContacts_ReturnsTrueAndRequestsContactTab()
    {
        string? requestPathAndQuery = null;
        string guidText = "e1b45d6c-7b45-4d11-aa52-b0a1cf2d6845";
        string html = $"""
            <dl class="profile-contacts">
                <dt class="Sub Item">Other contacts</dt>
                <dd class="mini_buttons Sub Item">{guidText}</dd>
            </dl>
            """;

        using IHost host = await new HostBuilder()
            .ConfigureWebHost(webHost => webHost.UseTestServer()
                .Configure(application => application.Run(async context =>
                {
                    requestPathAndQuery = $"{context.Request.Path}{context.Request.QueryString}";
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(html);
                })))
            .StartAsync();

        HttpClient httpClient = host.GetTestClient();
        httpClient.BaseAddress = new Uri("https://nill.forumcommunity.net/");
        ForumGuidVerificationService service = new(httpClient);
        Guid guid = Guid.Parse(guidText);

        bool isVerified = await service.VerifyAsync(7831486, guid);

        Assert.True(isVerified);
        Assert.Equal("/?act=Profile&MID=7831486&tab=contact", requestPathAndQuery);
    }

    [Fact]
    public async Task VerifyAsync_WhenGuidIsOutsideProfileContacts_ReturnsFalse()
    {
        string guidText = "e1b45d6c-7b45-4d11-aa52-b0a1cf2d6845";
        string html = $"""
            <script>const profileGuid = "{guidText}";</script>
            <dl class="profile-contacts"><dd>another contact</dd></dl>
            """;

        using IHost host = await new HostBuilder()
            .ConfigureWebHost(webHost => webHost.UseTestServer()
                .Configure(application => application.Run(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(html);
                })))
            .StartAsync();

        HttpClient httpClient = host.GetTestClient();
        ForumGuidVerificationService service = new(httpClient);
        Guid guid = Guid.Parse(guidText);

        bool isVerified = await service.VerifyAsync(7831486, guid);

        Assert.False(isVerified);
    }
}
