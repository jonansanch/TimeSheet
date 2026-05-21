using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;

[Authorize(Roles = Roles.Admin)]
public record GetBitacoraQuery(
    DateOnly? Desde,
    DateOnly? Hasta,
    string? ActorId,
    string? TipoEvento) : IRequest<BitacoraResponse>;
