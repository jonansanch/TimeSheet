namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IClock
{
    DateOnly Today { get; }
}
