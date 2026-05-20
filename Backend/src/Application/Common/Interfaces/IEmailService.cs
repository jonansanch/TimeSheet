namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
