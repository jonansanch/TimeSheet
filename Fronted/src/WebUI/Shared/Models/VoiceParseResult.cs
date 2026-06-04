namespace KPG.Timesheet.WebUI.Shared.Models;

public class VoiceParseResult
{
    public DateOnly? Fecha { get; set; }
    public TimeOnly? HoraEntradaAM { get; set; }
    public TimeOnly? HoraSalidaAM { get; set; }
    public TimeOnly? HoraEntradaPM { get; set; }
    public TimeOnly? HoraSalidaPM { get; set; }
    public string? Cliente { get; set; }
    public string? Proyecto { get; set; }
    public string? Modalidad { get; set; }
    public string? Recurso { get; set; }
    public string? Descripcion { get; set; }
    public string? Lugar { get; set; }
    public List<string> CamposNoDetectados { get; set; } = [];
}
