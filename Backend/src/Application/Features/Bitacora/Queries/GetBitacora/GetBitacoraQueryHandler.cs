using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;

public class GetBitacoraQueryHandler(IBitacoraQueryRepository repository)
    : IRequestHandler<GetBitacoraQuery, BitacoraResponse>
{
    public Task<BitacoraResponse> Handle(GetBitacoraQuery request, CancellationToken cancellationToken)
        => repository.GetAsync(request.Desde, request.Hasta, request.ActorId, request.TipoEvento, cancellationToken);
}
