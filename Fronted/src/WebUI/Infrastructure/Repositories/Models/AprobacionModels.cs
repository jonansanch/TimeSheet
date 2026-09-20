namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>Avance de un registro por la cadena de tres niveles.</summary>
public enum EstadoAprobacion
{
    Pendiente      = 0,
    AprobadoNivel1 = 1,
    AprobadoNivel2 = 2,
    Aprobado       = 3,
    Rechazado      = 4
}

public record PendientesAprobacionResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int TotalRegistros,
    decimal TotalHoras,
    List<DiaPendienteResponse> Dias);

/// <summary>
/// Un día de un empleado. Agrupa sus registros porque un mismo día puede repartirse
/// entre varios proyectos, y el supervisor revisa el día como unidad.
/// </summary>
public record DiaPendienteResponse(
    string UserId,
    string NombreEmpleado,
    DateOnly Fecha,
    int TotalMinutos,
    List<RegistroPendienteResponse> Registros);

public record RegistroPendienteResponse(
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
    int NivelDelRevisor);

public record EstadoRegistroResponse(
    int Id,
    EstadoAprobacion Estado,
    int? NivelPendiente,
    string? ComentarioRechazo);

public record RechazarRequest(string Comentario);

/// <summary>Resultado de importar un timesheet desde Excel.</summary>
public record ImportacionResultadoResponse(
    string? NombreEnArchivo,
    int Mes,
    int Anio,
    int FilasLeidas,
    int Importadas,
    List<FilaOmitidaResponse> Omitidas,
    List<FilaOmitidaResponse> Errores)
{
    public bool SinCambios => Importadas == 0;
}

public record FilaOmitidaResponse(int NumeroFila, DateOnly? Fecha, string Detalle, string Motivo);

/// <summary>Un empleado que este revisor puede aprobar.</summary>
public record EmpleadoRevisableResponse(string UserId, string Nombre);
