using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Application.Features.Bitacora.Queries.ExportarBitacora;

[Authorize(Roles = Roles.Admin)]
public record ExportarBitacoraQuery(
    DateOnly? Desde,
    DateOnly? Hasta,
    string? ActorId,
    string? TipoEvento) : IRequest<ExportarBitacoraResult>;

public record ExportarBitacoraResult(byte[] Contenido, string ContentType, string FileName);
