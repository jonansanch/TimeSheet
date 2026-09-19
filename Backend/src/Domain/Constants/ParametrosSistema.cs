namespace KPG.Timesheet.Domain.Constants;

public static class ParametrosSistema
{
    public const string VentanaRetroactividad = "VentanaRetroactividad";
    public const string DiasUmbralNotificacion = "DiasUmbralNotificacion";

    /// <summary>Horas minimas que debe sumar un dia para considerarse completo.</summary>
    public const string HorasDiaCompleto = "HorasDiaCompleto";

    /// <summary>Corte con el que el supervisor revisa: "Semanal" o "Quincenal".</summary>
    public const string PeriodoAprobacion = "PeriodoAprobacion";
}
