using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
