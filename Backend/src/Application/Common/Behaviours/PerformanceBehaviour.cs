using System.Diagnostics;
using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;

    public PerformanceBehaviour(ILogger<TRequest> logger, IUser user)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _user = user;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("KPG.Timesheet Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                typeof(TRequest).Name, elapsedMilliseconds, _user.Id ?? string.Empty, _user.Email ?? string.Empty, request);
        }

        return response;
    }
}
