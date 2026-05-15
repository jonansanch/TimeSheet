namespace KPG.Timesheet.Domain.Entities;

public class ParametroSistema : BaseAuditableEntity
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
