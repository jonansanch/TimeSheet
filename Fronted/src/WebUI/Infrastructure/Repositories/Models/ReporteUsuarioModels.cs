namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public enum TipoReporte
{
    Falla  = 1,
    Mejora = 2
}

public enum EstadoReporte
{
    Nuevo      = 1,
    EnRevision = 2,
    Resuelto   = 3,
    Rechazado  = 4
}

public record ReporteUsuarioResponse(
    int Id,
    string UserId,
    TipoReporte Tipo,
    string Titulo,
    string Descripcion,
    EstadoReporte Estado,
    string? ComentarioRespuesta,
    string? RespondidoPorUserId,
    DateTimeOffset Created);

public record CrearReporteUsuarioRequest(TipoReporte Tipo, string Titulo, string Descripcion);

public record CambiarEstadoReporteRequest(EstadoReporte NuevoEstado, string? Comentario);
