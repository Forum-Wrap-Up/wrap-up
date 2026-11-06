using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Data.Entities.Raw;
using ForumWrapUp.Importing;
using ForumWrapUp.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ForumWrapUp.Web.Services;

public sealed class AnnualWrapUpGenerationService
{
    private readonly ForumWrapUpDbContext _database;
    private readonly AnnualWrapUpGenerationOptions _options;

    public AnnualWrapUpGenerationService(
        ForumWrapUpDbContext database,
        IOptions<AnnualWrapUpGenerationOptions> options)
    {
        _database = database;
        _options = options.Value;
    }

    public async Task<AnnualWrapUp> GenerateAsync(int mid, int year, CancellationToken cancellationToken)
    {
        if (year < 1 || year >= DateOnly.MaxValue.Year)
            throw new ArgumentOutOfRangeException(nameof(year));

        bool userExists = await _database.UserAccounts.AnyAsync(account => account.Mid == mid, cancellationToken);
        bool yearExists = await _database.WrapUpYears.AnyAsync(item => item.Year == year, cancellationToken);
        if (!userExists || !yearExists)
            throw new KeyNotFoundException("The user or wrap-up year does not exist.");

        DateOnly firstDay = new(year, 1, 1);
        DateOnly lastDay = new(year, 12, 31);
        await EnsureImportAvailabilityAsync(mid, firstDay, lastDay, cancellationToken);

        ForumImportDateRange dateRange = ForumImportDateRange.Create(firstDay, lastDay);
        List<ForumPost> posts = await _database.ForumPosts
            .AsNoTracking()
            .Include(post => post.Topic)
            .Where(post =>
                post.AuthorMid == mid &&
                post.PublishedAtUtc >= dateRange.FromUtc &&
                post.PublishedAtUtc < dateRange.ToUtcExclusive)
            .ToListAsync(cancellationToken);

        long totalPoints = posts.Sum(post => (long)(post.Points ?? 0));
        ForumPost? mostSuccessfulPost = posts
            .OrderByDescending(post => post.Points ?? 0)
            .ThenByDescending(post => post.PublishedAtUtc)
            .ThenByDescending(post => post.ExternalId)
            .FirstOrDefault();

        IGrouping<long, ForumPost>? topTopicPosts = posts
            .Where(post => post.TopicId.HasValue)
            .GroupBy(post => post.TopicId.GetValueOrDefault())
            .OrderByDescending(topicPosts => topicPosts.Count())
            .ThenByDescending(topicPosts => topicPosts.Max(post => post.PublishedAtUtc))
            .ThenBy(topicPosts => topicPosts.Key)
            .FirstOrDefault();

        ForumTopic? topTopic = topTopicPosts?.Select(post => post.Topic).FirstOrDefault(topic => topic is not null);
        Dictionary<DateOnly, int> dailyPostCounts = posts
            .GroupBy(post => ForumLocalTime.ToDate(post.PublishedAtUtc))
            .ToDictionary(group => group.Key, group => group.Count());

        AnnualWrapUp? summary = await _database.AnnualWrapUps
            .Include(item => item.DailyPostStatistics)
            .SingleOrDefaultAsync(item => item.Mid == mid && item.Year == year, cancellationToken);

        if (summary is null)
        {
            summary = new AnnualWrapUp
            {
                Mid = mid,
                Year = year
            };
            _database.AnnualWrapUps.Add(summary);
        }

        summary.IsReady = true;
        summary.TotalPosts = posts.Count;
        summary.TotalPoints = totalPoints;
        summary.TopTopicExternalId = topTopic?.ExternalId;
        summary.TopTopicTitleHtml = topTopic?.TitleHtml;
        summary.TopTopicPostCount = topTopicPosts?.Count() ?? 0;
        summary.MostSuccessfulPostExternalId = mostSuccessfulPost?.ExternalId;
        summary.MostSuccessfulPostPoints = mostSuccessfulPost?.Points;
        summary.MostSuccessfulPostContentHtml = mostSuccessfulPost?.ContentHtml;
        summary.MostSuccessfulPostDateUtc = mostSuccessfulPost?.PublishedAtUtc;
        summary.CalculatedAtUtc = DateTime.UtcNow;

        UpdateDailyPostStatistics(summary, year, firstDay, lastDay, dailyPostCounts);
        await _database.SaveChangesAsync(cancellationToken);

        return summary;
    }

    private async Task EnsureImportAvailabilityAsync(int mid, DateOnly firstDay, DateOnly lastDay, CancellationToken cancellationToken)
    {
        List<ImportOperation> operations = await _database.ImportOperations
            .AsNoTracking()
            .Where(operation =>
                operation.Mid == mid &&
                operation.FromDate <= lastDay &&
                operation.ToDate >= firstDay &&
                (operation.Status == ImportOperationStatus.Queued ||
                 operation.Status == ImportOperationStatus.Running ||
                 operation.Status == ImportOperationStatus.Succeeded))
            .OrderBy(operation => operation.FromDate)
            .ThenByDescending(operation => operation.ToDate)
            .ToListAsync(cancellationToken);

        bool hasPendingImport = operations.Any(operation =>
            operation.Status == ImportOperationStatus.Queued ||
            operation.Status == ImportOperationStatus.Running);

        if (hasPendingImport)
            throw new AnnualWrapUpGenerationException("An import for this user and year is still queued or running.");

        if (!_options.RequireFullYearImportCoverage)
            return;

        DateOnly nextUncoveredDay = firstDay;
        foreach (ImportOperation operation in operations)
        {
            if (operation.Status != ImportOperationStatus.Succeeded || operation.ToDate < nextUncoveredDay)
                continue;

            if (operation.FromDate > nextUncoveredDay)
                break;

            if (operation.ToDate >= lastDay)
                return;

            nextUncoveredDay = operation.ToDate.AddDays(1);
        }

        throw new AnnualWrapUpGenerationException("A successful import covering the entire calendar year is required.");
    }

    private void UpdateDailyPostStatistics(AnnualWrapUp summary, int year, DateOnly firstDay, DateOnly lastDay, IReadOnlyDictionary<DateOnly, int> postCountsByDay)
    {
        List<DailyPostStatistic> obsoleteStatistics = summary.DailyPostStatistics
            .Where(statistic => statistic.Day.Year != year)
            .ToList();

        foreach (DailyPostStatistic obsoleteStatistic in obsoleteStatistics)
        {
            summary.DailyPostStatistics.Remove(obsoleteStatistic);
            _database.DailyPostStatistics.Remove(obsoleteStatistic);
        }

        Dictionary<DateOnly, DailyPostStatistic> existingStatistics = summary.DailyPostStatistics
            .ToDictionary(statistic => statistic.Day);

        for (DateOnly day = firstDay; day <= lastDay; day = day.AddDays(1))
        {
            int postCount = postCountsByDay.GetValueOrDefault(day);
            if (existingStatistics.TryGetValue(day, out DailyPostStatistic? statistic))
            {
                statistic.PostCount = postCount;
                continue;
            }

            DailyPostStatistic newStatistic = new()
            {
                Mid = summary.Mid,
                Year = summary.Year,
                Day = day,
                PostCount = postCount,
                AnnualWrapUp = summary
            };

            summary.DailyPostStatistics.Add(newStatistic);
            _database.DailyPostStatistics.Add(newStatistic);
        }
    }
}
