namespace KPG.Timesheet.WebUI.Shared.Models;

public class VoiceParseResult
{
    public DateOnly? Fecha { get; set; }
    public TimeOnly? HoraEntrada1 { get; set; }
    public TimeOnly? HoraSalida1 { get; set; }
    public TimeOnly? HoraEntrada2 { get; set; }
    public TimeOnly? HoraSalida2 { get; set; }
    public TimeOnly? HoraEntrada3 { get; set; }
    public TimeOnly? HoraSalida3 { get; set; }
    public string? Cliente { get; set; }
    public string? Proyecto { get; set; }
    public string? Modalidad { get; set; }
    public string? Recurso { get; set; }
    public string? Descripcion { get; set; }
    public string? Lugar { get; set; }
    public List<string> CamposNoDetectados { get; set; } = [];
}
