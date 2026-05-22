using System.Reflection;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;

namespace KPG.Timesheet.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUser _user;

    public AuthorizationBehaviour(IUser user)
    {
        _user = user;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            if (_user.Id == null)
                throw new UnauthorizedAccessException();

            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        if (_user.Roles?.Any(x => role == x) ?? false)
                        {
                            authorized = true;
                            break;
                        }
                    }
                }

                if (!authorized)
                    throw new ForbiddenAccessException();
            }
        }

        return await next();
    }
}
