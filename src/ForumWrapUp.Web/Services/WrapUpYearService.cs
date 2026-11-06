using ForumWrapUp.Data;
using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ForumWrapUp.Web.Services;

public sealed class WrapUpYearService
{
    private readonly ForumWrapUpDbContext _database;

    public WrapUpYearService(ForumWrapUpDbContext database)
    {
        _database = database;
    }

    public Task<List<WrapUpYearAvailability>> GetEnabledAvailabilityAsync(int mid, CancellationToken cancellationToken)
    {
        return _database.WrapUpYears
            .AsNoTracking()
            .Where(year => year.IsEnabled)
            .OrderByDescending(year => year.Year)
            .Select(year => new WrapUpYearAvailability(
                year.Year,
                year.AnnualWrapUps.Any(summary => summary.Mid == mid && summary.IsReady)))
            .ToListAsync(cancellationToken);
    }

    public Task<List<WrapUpYear>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _database.WrapUpYears
            .AsNoTracking()
            .OrderBy(year => year.Year)
            .ToListAsync(cancellationToken);
    }

    public async Task<WrapUpYear> SetEnabledAsync(int year, bool isEnabled, CancellationToken cancellationToken)
    {
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year));

        WrapUpYear? configuredYear = await _database.WrapUpYears
            .SingleOrDefaultAsync(item => item.Year == year, cancellationToken);

        if (configuredYear is null)
        {
            configuredYear = new WrapUpYear
            {
                Year = year,
                IsEnabled = isEnabled,
                CreatedAtUtc = DateTime.UtcNow
            };
            _database.WrapUpYears.Add(configuredYear);
        }
        else
            configuredYear.IsEnabled = isEnabled;

        await _database.SaveChangesAsync(cancellationToken);
        return configuredYear;
    }
}
