namespace KPG.Timesheet.Domain.Constants;

public static class ParametrosSistema
{
    public const string VentanaRetroactividad = "VentanaRetroactividad";
    public const string DiasUmbralNotificacion = "DiasUmbralNotificacion";

    /// <summary>Horas minimas que debe sumar un dia para considerarse completo.</summary>
    public const string HorasDiaCompleto = "HorasDiaCompleto";

    /// <summary>Corte con el que el supervisor revisa: "Semanal" o "Quincenal".</summary>
    public const string PeriodoAprobacion = "PeriodoAprobacion";

    /// <summary>
    /// Logo que se imprime en los reportes (timesheet y reporte de horas), como data URI
    /// completo ("data:image/png;base64,..."). Vacio o ausente significa sin logo.
    /// </summary>
    public const string LogoReportes = "LogoReportes";
}
