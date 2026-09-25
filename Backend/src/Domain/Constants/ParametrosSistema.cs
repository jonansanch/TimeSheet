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

    /// <summary>
    /// API key de Gemini usada para el dictado por voz y "mejorar redaccion" con IA. Vacio
    /// significa que esas funciones no estan disponibles. Se parametriza desde el propio
    /// sistema (pantalla de administracion) para poder renovarla el dia que venza, sin
    /// depender de un despliegue.
    /// </summary>
    public const string GeminiApiKey = "GeminiApiKey";

    // ── Calidad de descripciones (ver Docs/plan-calidad-descripciones.md) ──

    /// <summary>"true"/"false". Interruptor general: en false, IValidadorDescripcion nunca bloquea.</summary>
    public const string DescripcionValidacionActiva = "Descripcion.ValidacionActiva";

    /// <summary>
    /// Minimo de palabras (sin contar el prefijo "[Proyecto] - ") para no marcar la
    /// descripcion como MUY_CORTA.
    /// </summary>
    public const string DescripcionMinPalabras = "Descripcion.MinPalabras";

    /// <summary>
    /// Palabras "con contenido" (sin stopwords ni el nombre del proyecto) que deben quedar
    /// fuera de un termino GenericoSiVaSolo para no marcarlo.
    /// </summary>
    public const string DescripcionMinPalabrasContexto = "Descripcion.MinPalabrasContexto";

    /// <summary>"Advertir"/"Bloquear". Severidad de las reglas base (MUY_CORTA, MAYUSCULAS, etc.).</summary>
    public const string DescripcionSeveridadReglasBase = "Descripcion.SeveridadReglasBase";
}
