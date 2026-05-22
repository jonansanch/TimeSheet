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

    public static int CountBusinessDays(DateOnly from, DateOnly to)
    {
        int count = 0;
        var d = from.AddDays(1);
        while (d <= to)
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                count++;
            d = d.AddDays(1);
        }
        return Math.Min(count, 999);
    }
}
