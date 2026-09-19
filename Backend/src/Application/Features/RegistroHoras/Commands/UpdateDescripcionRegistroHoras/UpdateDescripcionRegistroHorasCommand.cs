using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

/// <summary>
/// Edicion de un registro por parte de un supervisor.
///
/// <para>
/// Solo se tocan los campos mutables del dia: descripcion y metadatos. Los bloques
/// horarios siguen siendo inmutables una vez registrados (ver
/// <c>RegistroHorasImmutabilityInterceptor</c>), y el proyecto identifica al registro.
/// </para>
/// <para>
/// Modalidad, recurso y lugar son opcionales: en null se dejan como estaban.
/// </para>
/// </summary>
[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record UpdateDescripcionRegistroHorasCommand(
    int RegistroId,
    string Descripcion,
    string? Modalidad = null,
    string? Recurso = null,
    string? Lugar = null) : IRequest;
