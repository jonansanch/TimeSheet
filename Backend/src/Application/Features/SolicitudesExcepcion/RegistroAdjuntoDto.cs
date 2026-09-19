using KPG.Timesheet.Domain.Entities;

// Fuera de una subcarpeta "Common": ese nombre ensombreceria a Application.Common
// dentro de la feature y romperia los usos de Common.Exceptions.
namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion;

/// <summary>
/// Resumen del registro que viaja adjunto a una solicitud. Quien aprueba ya no autoriza
/// solo una fecha: autoriza el registro que se va a crear, asi que tiene que poder verlo.
/// </summary>
public record RegistroAdjuntoDto(
    int      ProyectoId,
    string   Cliente,
    string   Proyecto,
    string   Horario,
    decimal  TotalHoras,
    string   Modalidad,
    string   Recurso,
    string   Lugar,
    string   Descripcion)
{
    public static RegistroAdjuntoDto? DesdeSolicitud(SolicitudExcepcion s)
    {
        if (!s.TieneRegistro) return null;

        var bloques = new[]
        {
            (s.HoraEntrada1, s.HoraSalida1),
            (s.HoraEntrada2, s.HoraSalida2),
            (s.HoraEntrada3, s.HoraSalida3)
        }.Where(b => b.Item1.HasValue && b.Item2.HasValue).ToList();

        var minutos = bloques.Sum(b => (int)(b.Item2!.Value - b.Item1!.Value).TotalMinutes);

        return new RegistroAdjuntoDto(
            s.ProyectoId!.Value,
            s.ClienteNombre ?? string.Empty,
            s.ProyectoNombre ?? string.Empty,
            string.Join(" / ", bloques.Select(b => $"{b.Item1:HH\\:mm}-{b.Item2:HH\\:mm}")),
            Math.Round(minutos / 60m, 2),
            s.Modalidad ?? string.Empty,
            s.Recurso ?? string.Empty,
            s.Lugar ?? string.Empty,
            s.Descripcion ?? string.Empty);
    }
}
