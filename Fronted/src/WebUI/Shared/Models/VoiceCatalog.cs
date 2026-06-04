namespace KPG.Timesheet.WebUI.Shared.Models;

public class VoiceCatalog
{
    public List<string> Clientes { get; set; } = [];
    public Dictionary<string, List<string>> ProyectosPorCliente { get; set; } = [];
    public List<string> Modalidades { get; set; } = [];
    public List<string> Recursos { get; set; } = [];
    public List<string> Lugares { get; set; } = [];
}
