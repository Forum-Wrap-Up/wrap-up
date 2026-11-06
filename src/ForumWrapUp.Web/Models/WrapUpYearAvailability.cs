namespace ForumWrapUp.Web.Models;

public sealed class WrapUpYearAvailability
{
    public WrapUpYearAvailability(int year, bool isAvailable)
    {
        Year = year;
        IsAvailable = isAvailable;
    }

    public int Year { get; }

    public bool IsAvailable { get; }
}
