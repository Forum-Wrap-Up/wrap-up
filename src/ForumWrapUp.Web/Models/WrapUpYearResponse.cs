using ForumWrapUp.Data.Entities;

namespace ForumWrapUp.Web.Models;

public sealed class WrapUpYearResponse
{
    public WrapUpYearResponse(int year, bool isEnabled)
    {
        Year = year;
        IsEnabled = isEnabled;
    }

    public int Year { get; }

    public bool IsEnabled { get; }

    public static WrapUpYearResponse FromYear(WrapUpYear year)
    {
        return new WrapUpYearResponse(year.Year, year.IsEnabled);
    }
}
