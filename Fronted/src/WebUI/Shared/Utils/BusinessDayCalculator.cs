using KPG.Timesheet.WebUI.Shared.Models;

namespace KPG.Timesheet.WebUI.Shared.Utils;

public static class BusinessDayCalculator
{
    public static DateOnly GetEarliestAllowedDate(DateOnly referenceDate, int windowDays)
    {
        var current = referenceDate;
        var remaining = windowDays;
        while (remaining > 0)
        {
            current = current.AddDays(-1);
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                remaining--;
        }
        return current;
    }

    public static DateAvailability GetAvailability(DateOnly date, DateOnly today, DateOnly earliestAllowed)
    {
        if (date >= today) return DateAvailability.Available;
        if (date >= earliestAllowed) return DateAvailability.Retroactive;
        return DateAvailability.OutOfWindow;
    }
}
