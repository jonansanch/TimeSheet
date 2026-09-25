using System.Text.RegularExpressions;
using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Common.Services;

/// <summary>Codigos de las reglas base (no dependen del catalogo de terminos).</summary>
public static class CodigoHallazgoDescripcion
{
    public const string MuyCorta          = "MUY_CORTA";
    public const string Mayusculas        = "MAYUSCULAS";
    public const string CaracteresRepetidos = "REPETIDOS";
    public const string SinLetras         = "SIN_LETRAS";
    public const string MarcadorSinLlenar = "MARCADOR_SIN_LLENAR";
}

/// <summary>
/// Parametros de <c>ParametroSistema</c> (ver <see cref="Domain.Constants.ParametrosSistema"/>)
/// que gobiernan la evaluacion. Se cargan una vez por llamado y se pasan como valor: el
/// evaluador en si no toca la base de datos.
/// </summary>
public record ParametrosEvaluacionDescripcion(
    bool ValidacionActiva,
    int MinPalabras,
    int MinPalabrasContexto,
    SeveridadTermino SeveridadReglasBase)
{
    /// <summary>Los mismos valores con los que arranca el catalogo semilla (ver Fase 0).</summary>
    public static ParametrosEvaluacionDescripcion PorDefecto { get; } = new(
        ValidacionActiva: true,
        MinPalabras: 4,
        MinPalabrasContexto: 3,
        SeveridadReglasBase: SeveridadTermino.Advertir);
}

/// <summary>Un problema de calidad encontrado en una descripcion.</summary>
public record HallazgoDescripcion(
    string Codigo,
    SeveridadTermino Severidad,
    string Mensaje,
    string? Fragmento,
    string? Sugerencia);

/// <summary>Resultado de evaluar una descripcion: todos los hallazgos, en el orden en que se detectaron.</summary>
public record EvaluacionDescripcion(IReadOnlyList<HallazgoDescripcion> Hallazgos)
{
    /// <summary>True si algun hallazgo es <see cref="SeveridadTermino.Bloquear"/>: el guardado debe impedirse.</summary>
    public bool Bloquea => Hallazgos.Any(h => h.Severidad == SeveridadTermino.Bloquear);

    public static EvaluacionDescripcion Vacia { get; } = new([]);
}

/// <summary>
/// Evalua la calidad de una descripcion de registro de horas contra el catalogo de
/// <see cref="TerminoDescripcion"/> y un puñado de reglas base fijas. Es logica pura: no
/// toca la base de datos ni el reloj, para poder probarla sin infraestructura. Ver
/// Docs/plan-calidad-descripciones.md.
///
/// <para>
/// Lo usan por igual el validador de los comandos de guardado (bloquea si corresponde) y
/// el endpoint <c>/api/descripciones/evaluar</c> del aviso en vivo del formulario: misma
/// entrada, mismo resultado siempre.
/// </para>
/// </summary>
public static class EvaluadorDescripcion
{
    /// <summary>
    /// Palabras sin contenido propio: no cuentan como "contexto" para la regla
    /// <see cref="ReglaTermino.GenericoSiVaSolo"/>. Lista corta a proposito, solo las mas
    /// comunes en español; no pretende ser un lematizador.
    /// </summary>
    private static readonly HashSet<string> Stopwords =
    [
        "de", "del", "la", "el", "los", "las", "en", "con", "para", "por", "un", "una",
        "unos", "unas", "y", "o", "a", "al", "su", "sus", "que", "se", "sin", "sobre",
        "entre", "lo", "le", "les", "este", "esta", "estos", "estas", "es", "ha", "han"
    ];

    private static readonly Regex RegexRepetidos = new(@"(.)\1{3,}", RegexOptions.Compiled);
    private static readonly Regex RegexMarcador  = new(@"\[[^\[\]]*\]", RegexOptions.Compiled);
    private static readonly Regex RegexLetras    = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex RegexPalabras  = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

    public static EvaluacionDescripcion Evaluar(
        string? texto,
        IReadOnlyCollection<TerminoDescripcion> terminosActivos,
        ParametrosEvaluacionDescripcion parametros,
        string? nombreProyecto = null)
    {
        // Interruptor general: ni avisa ni bloquea. Lo usa el equipo para apagar la
        // funcionalidad completa sin tocar el catalogo termino por termino.
        if (!parametros.ValidacionActiva)
            return EvaluacionDescripcion.Vacia;

        var textoOriginal = (texto ?? string.Empty).Trim();
        var hallazgos = new List<HallazgoDescripcion>();

        EvaluarTerminos(textoOriginal, terminosActivos, parametros, nombreProyecto, hallazgos);
        EvaluarReglasBase(textoOriginal, parametros, nombreProyecto, hallazgos);

        return new EvaluacionDescripcion(hallazgos);
    }

    private static void EvaluarTerminos(
        string texto,
        IReadOnlyCollection<TerminoDescripcion> terminosActivos,
        ParametrosEvaluacionDescripcion parametros,
        string? nombreProyecto,
        List<HallazgoDescripcion> hallazgos)
    {
        var normalizado = NormalizadorTexto.Normalizar(texto);
        if (normalizado.Length == 0)
            return;

        var proyectoNormalizado = string.IsNullOrWhiteSpace(nombreProyecto)
            ? null
            : NormalizadorTexto.Normalizar(nombreProyecto);

        // Solo tokens con letras/numeros: un "-" suelto (separador del formato
        // "[Proyecto] - [Accion]") no debe contar como palabra de contexto.
        var tokens = RegexPalabras.Matches(normalizado).Select(m => m.Value).ToArray();

        foreach (var termino in terminosActivos.Where(t => t.Activo))
        {
            if (!Coincide(normalizado, termino.TerminoNormalizado))
                continue;

            var seMarca = termino.Regla == ReglaTermino.ProhibidoSiempre
                || ContarPalabrasDeContexto(tokens, termino.TerminoNormalizado, proyectoNormalizado) < parametros.MinPalabrasContexto;

            if (!seMarca)
                continue;

            hallazgos.Add(new HallazgoDescripcion(
                Codigo: $"TERMINO:{termino.TerminoNormalizado}",
                Severidad: termino.Severidad,
                Mensaje: termino.Motivo,
                Fragmento: termino.Termino,
                Sugerencia: termino.Sugerencia));
        }
    }

    /// <summary>Coincidencia de palabra completa: "soporte" no coincide dentro de "soportes".</summary>
    private static bool Coincide(string textoNormalizado, string terminoNormalizado) =>
        Regex.IsMatch(textoNormalizado, $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(terminoNormalizado)}(?![\p{{L}}\p{{N}}])");

    /// <summary>
    /// Palabras que quedan si se descuentan el termino, el nombre del proyecto y las
    /// stopwords. Es una resta de multiconjuntos simple, no un analisis gramatical: alcanza
    /// para distinguir "Reunion" (0 palabras de contexto) de "Reunion de planificacion del
    /// sprint con el cliente" (varias).
    /// </summary>
    private static int ContarPalabrasDeContexto(
        string[] tokens, string terminoNormalizado, string? proyectoNormalizado)
    {
        var aDescontar = terminoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (proyectoNormalizado is not null)
            aDescontar.AddRange(proyectoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        var contexto = 0;
        foreach (var token in tokens)
        {
            var idx = aDescontar.IndexOf(token);
            if (idx >= 0)
            {
                aDescontar.RemoveAt(idx);
                continue;
            }

            if (!Stopwords.Contains(token))
                contexto++;
        }

        return contexto;
    }

    private static void EvaluarReglasBase(
        string texto,
        ParametrosEvaluacionDescripcion parametros,
        string? nombreProyecto,
        List<HallazgoDescripcion> hallazgos)
    {
        var severidad = parametros.SeveridadReglasBase;

        if (!RegexLetras.IsMatch(texto))
        {
            // Sin letras: las demas reglas (mayusculas, palabras) no aportan nada mas.
            hallazgos.Add(new HallazgoDescripcion(
                CodigoHallazgoDescripcion.SinLetras, severidad,
                "La descripcion no tiene texto legible.", texto, null));
            return;
        }

        var sinPrefijo = QuitarPrefijoProyecto(texto, nombreProyecto);
        var palabras = RegexPalabras.Matches(sinPrefijo).Count;
        if (palabras < parametros.MinPalabras)
        {
            hallazgos.Add(new HallazgoDescripcion(
                CodigoHallazgoDescripcion.MuyCorta, severidad,
                $"La descripcion es muy corta ({palabras} palabra(s)); se recomiendan al menos {parametros.MinPalabras}.",
                sinPrefijo, null));
        }

        var letras = texto.Where(char.IsLetter).ToList();
        if (letras.Count > 10 && letras.All(char.IsUpper))
        {
            hallazgos.Add(new HallazgoDescripcion(
                CodigoHallazgoDescripcion.Mayusculas, severidad,
                "La descripcion esta en mayusculas; usa mayusculas y minusculas normales.",
                texto, null));
        }

        var repetidos = RegexRepetidos.Match(texto);
        if (repetidos.Success)
        {
            hallazgos.Add(new HallazgoDescripcion(
                CodigoHallazgoDescripcion.CaracteresRepetidos, severidad,
                "La descripcion tiene caracteres repetidos sin sentido.",
                repetidos.Value, null));
        }

        if (RegexMarcador.IsMatch(texto))
        {
            hallazgos.Add(new HallazgoDescripcion(
                CodigoHallazgoDescripcion.MarcadorSinLlenar, severidad,
                "Quedaron marcadores [asi] sin completar de una sugerencia aplicada.",
                RegexMarcador.Match(texto).Value, null));
        }
    }

    /// <summary>
    /// Quita el prefijo "Proyecto - " o "Proyecto – " (guion o guion largo) del conteo de
    /// palabras: es el formato que sugiere la guia y no deberia penalizar al usuario que lo
    /// sigue.
    /// </summary>
    private static string QuitarPrefijoProyecto(string texto, string? nombreProyecto)
    {
        if (string.IsNullOrWhiteSpace(nombreProyecto))
            return texto;

        var patron = $@"^\s*{Regex.Escape(nombreProyecto.Trim())}\s*[-–]\s*";
        return Regex.Replace(texto, patron, string.Empty, RegexOptions.IgnoreCase);
    }
}
