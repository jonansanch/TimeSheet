namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Lee el Excel de timesheet que usan los consultores y lo convierte en filas planas.
/// Solo interpreta el archivo: no valida contra catalogos ni toca la base de datos.
/// </summary>
public interface ITimesheetImportParser
{
    TimesheetImportado Parse(Stream contenido);
}

/// <summary>Contenido del archivo ya interpretado.</summary>
/// <param name="NombreConsultor">Nombre de la cabecera, para avisar si no coincide con el empleado elegido.</param>
/// <param name="Filas">Filas con datos, en el orden del archivo.</param>
/// <param name="Errores">Problemas de formato que impidieron leer filas concretas.</param>
public record TimesheetImportado(
    string? NombreConsultor,
    int? Mes,
    int? Anio,
    IReadOnlyList<FilaTimesheet> Filas,
    IReadOnlyList<ErrorImportacion> Errores);

public record FilaTimesheet(
    int NumeroFila,
    DateOnly Fecha,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Lugar,
    string Descripcion);

public record ErrorImportacion(int NumeroFila, string Motivo);

/// <summary>El archivo no tiene la forma esperada y no se pudo leer nada.</summary>
public class TimesheetImportFormatException(string message) : Exception(message);
