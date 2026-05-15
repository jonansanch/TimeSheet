namespace KPG.Timesheet.Domain.Common;

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
}
