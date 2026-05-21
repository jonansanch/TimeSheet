using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacoraAlcance;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente}")]
public record GetBitacoraAlcanceQuery(
    DateOnly? Desde,
    DateOnly? Hasta,
    string? ActorId,
    string? TipoEvento) : IRequest<BitacoraResponse>;
