using KPG.Timesheet.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user)
    {
        _logger = logger;
        _user = user;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("KPG.Timesheet Request: {Name} {@UserId} {@UserName} {@Request}",
            typeof(TRequest).Name, _user.Id ?? string.Empty, _user.Email ?? string.Empty, request);

        return Task.CompletedTask;
    }
}
