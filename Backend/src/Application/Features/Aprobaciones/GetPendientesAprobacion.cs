using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Features.Aprobaciones;

/// <summary>
/// Registros que esperan la aprobacion del usuario autenticado, dentro de un rango.
/// Los filtros son opcionales y se combinan entre si.
/// </summary>
[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetPendientesAprobacionQuery(
    DateOnly Desde,
    DateOnly Hasta,
    string? UserId = null,
    int? ClienteId = null,
    int? ProyectoId = null,
    int? PuestoId = null,
    bool IncluirRevisados = false) : IRequest<PendientesAprobacionResponse>;

public record PendientesAprobacionResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int TotalRegistros,
    decimal TotalHoras,
    IReadOnlyList<DiaPendienteDto> Dias);

/// <summary>
/// Un dia de un empleado. Agrupa sus registros porque un mismo dia puede repartirse
/// entre varios proyectos, y el supervisor revisa el dia como unidad.
/// </summary>
public record DiaPendienteDto(
    string UserId,
    string NombreEmpleado,
    DateOnly Fecha,
    int TotalMinutos,
    IReadOnlyList<RegistroPendienteDto> Registros);

public record RegistroPendienteDto(
    int Id,
    int ProyectoId,
    string Cliente,
    string Proyecto,
    string Recurso,
    string Modalidad,
    string Descripcion,
    int TotalMinutos,
    EstadoAprobacion Estado,
    int? NivelPendiente,
    string? ComentarioRechazo,
    /// <summary>Nivel que le toca revisar al usuario autenticado en este registro.</summary>
    int NivelDelRevisor,
    /// <summary>
    /// True si la descripcion tiene algun hallazgo de calidad (ver
    /// Docs/plan-calidad-descripciones.md). Se calcula al leer, contra el catalogo
    /// vigente: no es un valor guardado, asi que siempre refleja las reglas actuales,
    /// incluso sobre registros historicos.
    /// </summary>
    bool TieneObservacionesDescripcion);
