namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

// Mismo orden que Domain.Enums en el backend (TipoTermino, ReglaTermino, SeveridadTermino):
// los enums viajan como numero por JSON, asi que el orden tiene que coincidir exactamente.

public enum TipoTermino
{
    Palabra = 0,
    Frase = 1
}

public enum ReglaTermino
{
    ProhibidoSiempre = 0,
    GenericoSiVaSolo = 1
}

public enum SeveridadTermino
{
    Advertir = 0,
    Bloquear = 1
}

public record TerminoDescripcionResponse(
    int Id,
    string Termino,
    TipoTermino Tipo,
    ReglaTermino Regla,
    SeveridadTermino Severidad,
    string? Sugerencia,
    string Motivo,
    bool Activo);

public record CreateTerminoDescripcionRequest(
    string Termino, TipoTermino Tipo, ReglaTermino Regla, SeveridadTermino Severidad, string Motivo, string? Sugerencia);

public record UpdateTerminoDescripcionRequest(
    string Termino, TipoTermino Tipo, ReglaTermino Regla, SeveridadTermino Severidad, string Motivo, string? Sugerencia);

public record ParametrosDescripcionResponse(
    bool ValidacionActiva, int MinPalabras, int MinPalabrasContexto, SeveridadTermino SeveridadReglasBase);

public record UpdateParametrosDescripcionRequest(
    bool ValidacionActiva, int MinPalabras, int MinPalabrasContexto, SeveridadTermino SeveridadReglasBase);

public record EvaluarDescripcionRequest(string? Texto, int? ProyectoId);

public record HallazgoDescripcionResponse(
    string Codigo, string Severidad, string Mensaje, string? Fragmento, string? Sugerencia);

public record EvaluacionDescripcionResponse(bool Bloquea, List<HallazgoDescripcionResponse> Hallazgos);

public record MejorarDescripcionRequest(string Texto, int? ProyectoId);

public record MejorarDescripcionResultResponse(
    bool Disponible,
    string? Propuesta,
    List<string> Cambios,
    bool PropuestaBloquea,
    List<string> ObservacionesPropuesta);
