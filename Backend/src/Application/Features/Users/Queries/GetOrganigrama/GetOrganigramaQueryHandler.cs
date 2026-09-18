using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Users.Queries.GetOrganigrama;

public class GetOrganigramaQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetOrganigramaQuery, IReadOnlyList<OrganigramaNodoDto>>
{
    public Task<IReadOnlyList<OrganigramaNodoDto>> Handle(
        GetOrganigramaQuery request,
        CancellationToken cancellationToken)
        => identityService.GetOrganigramaAsync(cancellationToken);
}
