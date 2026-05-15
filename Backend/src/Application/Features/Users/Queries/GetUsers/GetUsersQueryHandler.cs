using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UsersPageDto>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<UsersPageDto> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _identityService.GetUsersAsync(
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDescending,
            cancellationToken);
}
