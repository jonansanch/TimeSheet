using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacoraAlcance;

public class GetBitacoraAlcanceQueryHandler(IBitacoraQueryRepository repository, IUser user)
    : IRequestHandler<GetBitacoraAlcanceQuery, BitacoraResponse>
{
    public Task<BitacoraResponse> Handle(GetBitacoraAlcanceQuery request, CancellationToken cancellationToken)
    {
        bool soloEquipo = user.Roles?.Contains(Roles.Supervisor) == true
                       && user.Roles?.Contains(Roles.Gerente) != true;

        return repository.GetAlcanceAsync(request.Desde, request.Hasta, request.ActorId, request.TipoEvento, soloEquipo, cancellationToken);
    }
}
