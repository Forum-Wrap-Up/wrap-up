using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Data.Entities.Raw;
using ForumWrapUp.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ForumWrapUp.Web.Tests;

public sealed class AnnualWrapUpGenerationTests
{
    private const string AdminToken = "test-admin-token";

    [Fact]
    public async Task AdminCanGenerateAndRegenerateFullYearAggregates()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedCalculationDataAsync(application);

        await using WebApplicationFactory<Program> adminApplication = CreateAdminApplication(application);
        WebApplicationFactoryClientOptions clientOptions = new()
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        };
        using HttpClient anonymousClient = adminApplication.CreateClient(clientOptions);
        using HttpResponseMessage anonymousResponse = await anonymousClient.PostAsync(
            "/api/admin/wrap-ups/123/2028/generate",
            content: null);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using HttpClient adminClient = adminApplication.CreateClient(clientOptions);
        using HttpRequestMessage request = CreateAdminRequest("/api/admin/wrap-ups/123/2028/generate");
        using HttpResponseMessage response = await adminClient.SendAsync(request);
        AnnualWrapUpGenerationResponse? result = await response.Content.ReadFromJsonAsync<AnnualWrapUpGenerationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(123, result.Mid);
        Assert.Equal(2028, result.Year);
        Assert.Equal(6, result.TotalPosts);
        Assert.Equal(40, result.TotalPoints);
        Assert.Equal(101, result.TopTopicExternalId);
        Assert.Equal(3, result.TopTopicPostCount);
        Assert.Equal(2005, result.MostSuccessfulPostExternalId);
        Assert.Equal(7, result.MostSuccessfulPostPoints);
        Assert.NotNull(result.CalculatedAtUtc);

        using IServiceScope scope = adminApplication.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        AnnualWrapUp? summary = await database.AnnualWrapUps
            .Include(item => item.DailyPostStatistics)
            .SingleOrDefaultAsync(item => item.Mid == 123 && item.Year == 2028);

        Assert.NotNull(summary);
        Assert.True(summary.IsReady);
        Assert.Equal(366, summary.DailyPostStatistics.Count);
        Assert.Equal(1, Assert.Single(summary.DailyPostStatistics, item => item.Day == new DateOnly(2028, 1, 1)).PostCount);
        Assert.Equal(3, Assert.Single(summary.DailyPostStatistics, item => item.Day == new DateOnly(2028, 1, 2)).PostCount);
        Assert.Equal(2, Assert.Single(summary.DailyPostStatistics, item => item.Day == new DateOnly(2028, 12, 31)).PostCount);
        Assert.Equal(0, Assert.Single(summary.DailyPostStatistics, item => item.Day == new DateOnly(2028, 2, 1)).PostCount);

        using HttpRequestMessage regenerateRequest = CreateAdminRequest("/api/admin/wrap-ups/123/2028/generate");
        using HttpResponseMessage regenerateResponse = await adminClient.SendAsync(regenerateRequest);
        Assert.Equal(HttpStatusCode.OK, regenerateResponse.StatusCode);

        List<AnnualWrapUp> summaries = await database.AnnualWrapUps
            .Include(item => item.DailyPostStatistics)
            .Where(item => item.Mid == 123 && item.Year == 2028)
            .ToListAsync();
        Assert.Single(summaries);
        Assert.Equal(366, summaries[0].DailyPostStatistics.Count);
    }

    [Fact]
    public async Task GenerationRequiresSuccessfulImportsCoveringTheWholeYear()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedIncompleteImportAsync(application);

        await using WebApplicationFactory<Program> adminApplication = CreateAdminApplication(application);
        using HttpClient adminClient = adminApplication.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using HttpRequestMessage request = CreateAdminRequest("/api/admin/wrap-ups/123/2026/generate");
        using HttpResponseMessage response = await adminClient.SendAsync(request);
        string problem = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("successful import covering the entire calendar year is required", problem);

        using IServiceScope scope = adminApplication.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        Assert.Empty(await database.AnnualWrapUps.ToListAsync());
    }

    [Fact]
    public async Task GenerationIsBlockedWhileAnImportIsQueued()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedQueuedImportAsync(application);

        await using WebApplicationFactory<Program> adminApplication = CreateAdminApplication(application, false);
        using HttpClient adminClient = adminApplication.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using HttpRequestMessage request = CreateAdminRequest("/api/admin/wrap-ups/123/2026/generate");
        using HttpResponseMessage response = await adminClient.SendAsync(request);
        string problem = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("still queued or running", problem);
    }

    [Fact]
    public async Task FullYearCoverageCanBeDisabledForPartialDevelopmentData()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedIncompleteImportAsync(application);

        await using WebApplicationFactory<Program> adminApplication = CreateAdminApplication(application, false);
        using HttpClient adminClient = adminApplication.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using HttpRequestMessage request = CreateAdminRequest("/api/admin/wrap-ups/123/2026/generate");
        using HttpResponseMessage response = await adminClient.SendAsync(request);
        AnnualWrapUpGenerationResponse? result = await response.Content.ReadFromJsonAsync<AnnualWrapUpGenerationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPosts);

        using IServiceScope scope = adminApplication.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        AnnualWrapUp? summary = await database.AnnualWrapUps.SingleOrDefaultAsync(item => item.Mid == 123 && item.Year == 2026);
        Assert.NotNull(summary);
        Assert.True(summary.IsReady);
        Assert.Equal(365, await database.DailyPostStatistics.CountAsync(item => item.Mid == 123 && item.Year == 2026));
    }

    [Fact]
    public async Task SuccessfulImportWithNoPostsGeneratesZeroCountsForEveryDay()
    {
        await using LoginWebApplicationFactory application = new();
        await application.SeedAccountsAsync();
        await SeedEmptyYearAsync(application);

        await using WebApplicationFactory<Program> adminApplication = CreateAdminApplication(application);
        using HttpClient adminClient = adminApplication.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using HttpRequestMessage request = CreateAdminRequest("/api/admin/wrap-ups/123/2026/generate");
        using HttpResponseMessage response = await adminClient.SendAsync(request);
        AnnualWrapUpGenerationResponse? result = await response.Content.ReadFromJsonAsync<AnnualWrapUpGenerationResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPosts);
        Assert.Equal(0, result.TotalPoints);
        Assert.Null(result.TopTopicExternalId);
        Assert.Null(result.MostSuccessfulPostExternalId);

        using IServiceScope scope = adminApplication.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        List<DailyPostStatistic> dailyStatistics = await database.DailyPostStatistics
            .Where(item => item.Mid == 123 && item.Year == 2026)
            .ToListAsync();

        Assert.Equal(365, dailyStatistics.Count);
        Assert.All(dailyStatistics, statistic => Assert.Equal(0, statistic.PostCount));
    }

    private static async Task SeedCalculationDataAsync(LoginWebApplicationFactory application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        UserAccount user = await database.UserAccounts.SingleAsync(account => account.Mid == 123);
        UserAccount otherUser = await database.UserAccounts.SingleAsync(account => account.Mid == 789);
        ForumInstance forum = new() { ApiForumId = 50 };
        ForumTopic firstTopic = new()
        {
            ApiForumId = forum.ApiForumId,
            ExternalId = 101,
            TitleHtml = "Topic uno",
            ForumInstance = forum
        };
        ForumTopic secondTopic = new()
        {
            ApiForumId = forum.ApiForumId,
            ExternalId = 202,
            TitleHtml = "Topic due",
            ForumInstance = forum
        };
        database.WrapUpYears.Add(new WrapUpYear
        {
            Year = 2028,
            IsEnabled = false,
            CreatedAtUtc = DateTime.UtcNow
        });
        database.ForumInstances.Add(forum);
        database.ForumTopics.AddRange(firstTopic, secondTopic);
        database.ImportOperations.AddRange(
            CreateSuccessfulImport(123, new DateOnly(2028, 1, 1), new DateOnly(2028, 6, 30)),
            CreateSuccessfulImport(123, new DateOnly(2028, 7, 1), new DateOnly(2028, 12, 31)));
        await database.SaveChangesAsync();

        AddPost(database, user, forum, firstTopic, 2001, new DateTime(2027, 12, 31, 23, 30, 0, DateTimeKind.Utc), 5);
        AddPost(database, user, forum, firstTopic, 2002, new DateTime(2028, 1, 2, 10, 0, 0, DateTimeKind.Utc), 7);
        AddPost(database, user, forum, secondTopic, 2003, new DateTime(2028, 1, 2, 11, 0, 0, DateTimeKind.Utc), 7);
        AddPost(database, user, forum, secondTopic, 2004, new DateTime(2028, 1, 2, 12, 0, 0, DateTimeKind.Utc), 7);
        AddPost(database, user, forum, firstTopic, 2005, new DateTime(2028, 12, 31, 22, 30, 0, DateTimeKind.Utc), 7);
        AddPost(database, user, forum, secondTopic, 2006, new DateTime(2028, 12, 31, 22, 0, 0, DateTimeKind.Utc), 7);
        AddPost(database, user, forum, firstTopic, 2007, new DateTime(2027, 12, 31, 22, 30, 0, DateTimeKind.Utc), 500);
        AddPost(database, user, forum, firstTopic, 2008, new DateTime(2028, 12, 31, 23, 30, 0, DateTimeKind.Utc), 500);
        AddPost(database, otherUser, forum, firstTopic, 2009, new DateTime(2028, 1, 2, 13, 0, 0, DateTimeKind.Utc), 900);
        await database.SaveChangesAsync();
    }

    private static async Task SeedIncompleteImportAsync(LoginWebApplicationFactory application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        database.WrapUpYears.Add(new WrapUpYear
        {
            Year = 2026,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        database.ImportOperations.Add(CreateSuccessfulImport(
            123,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30)));
        await database.SaveChangesAsync();
    }

    private static async Task SeedEmptyYearAsync(LoginWebApplicationFactory application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        database.WrapUpYears.Add(new WrapUpYear
        {
            Year = 2026,
            IsEnabled = false,
            CreatedAtUtc = DateTime.UtcNow
        });
        database.ImportOperations.Add(CreateSuccessfulImport(
            123,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31)));
        await database.SaveChangesAsync();
    }

    private static async Task SeedQueuedImportAsync(LoginWebApplicationFactory application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        ForumWrapUpDbContext database = scope.ServiceProvider.GetRequiredService<ForumWrapUpDbContext>();
        database.WrapUpYears.Add(new WrapUpYear
        {
            Year = 2026,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        ImportOperation queuedImport = new()
        {
            Id = Guid.NewGuid(),
            Mid = 123,
            FromDate = new DateOnly(2026, 1, 1),
            ToDate = new DateOnly(2026, 12, 31),
            Status = ImportOperationStatus.Queued,
            RequestedAtUtc = DateTime.UtcNow
        };
        database.ImportOperations.Add(queuedImport);
        await database.SaveChangesAsync();
    }

    private static ImportOperation CreateSuccessfulImport(int mid, DateOnly fromDate, DateOnly toDate)
    {
        DateTime requestedAtUtc = DateTime.UtcNow;
        return new ImportOperation
        {
            Id = Guid.NewGuid(),
            Mid = mid,
            FromDate = fromDate,
            ToDate = toDate,
            Status = ImportOperationStatus.Succeeded,
            RequestedAtUtc = requestedAtUtc,
            StartedAtUtc = requestedAtUtc,
            CompletedAtUtc = requestedAtUtc
        };
    }

    private static void AddPost(
        ForumWrapUpDbContext database,
        UserAccount user,
        ForumInstance forum,
        ForumTopic topic,
        int externalId,
        DateTime publishedAtUtc,
        int points)
    {
        ForumPost post = new()
        {
            ApiForumId = forum.ApiForumId,
            ExternalId = externalId,
            AuthorMid = user.Mid,
            PublishedAtUtc = publishedAtUtc,
            Points = points,
            RawPoints = points.ToString(),
            ContentHtml = $"Post {externalId}",
            ForumInstance = forum,
            Author = user,
            TopicId = topic.Id,
            Topic = topic
        };
        database.ForumPosts.Add(post);
    }

    private static WebApplicationFactory<Program> CreateAdminApplication(
        LoginWebApplicationFactory application,
        bool? requireFullYearImportCoverage = null)
    {
        return application.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                Dictionary<string, string?> settings = new()
                {
                    ["AdminApi:Token"] = AdminToken
                };
                if (requireFullYearImportCoverage.HasValue)
                    settings["AnnualWrapUpGeneration:RequireFullYearImportCoverage"] = requireFullYearImportCoverage.Value.ToString();

                configuration.AddInMemoryCollection(settings);
            }));
    }

    private static HttpRequestMessage CreateAdminRequest(string uri)
    {
        HttpRequestMessage request = new(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);
        return request;
    }
}
