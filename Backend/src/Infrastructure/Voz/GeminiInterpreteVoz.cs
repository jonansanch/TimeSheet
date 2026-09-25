using System.Text.Json;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Infrastructure.Ia;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Infrastructure.Voz;

public class GeminiSettings
{
    public string Model { get; set; } = "gemini-flash-latest";

    /// <summary>Un dictado es una frase corta: no hace falta pensar mucho ni escribir mucho.</summary>
    public int MaxTokens { get; set; } = 1024;
}

/// <summary>
/// Interpreta el dictado con Gemini usando salida estructurada: el modelo devuelve
/// directamente el JSON con la forma que espera el formulario, sin texto alrededor que
/// haya que recortar.
///
/// <para>
/// No inventa valores de catalogo: el prompt le pasa los clientes, proyectos, modalidades,
/// recursos y lugares reales y le exige elegir de esas listas o dejar el campo nulo.
/// </para>
///
/// <para>
/// La API key no vive en configuracion: se lee de <see cref="Domain.Entities.ParametroSistema"/>
/// en cada llamada, para que se pueda renovar desde la pantalla de administracion el dia que
/// venza, sin necesidad de un despliegue.
/// </para>
/// </summary>
public class GeminiInterpreteVoz(
    GeminiApiClient cliente,
    IParametrosSistemaService parametros,
    IOptions<GeminiSettings> settings,
    ILogger<GeminiInterpreteVoz> logger) : IInterpreteVoz
{
    private readonly GeminiSettings _settings = settings.Value;

    public async Task<bool> DisponibleAsync(CancellationToken cancellationToken = default) =>
        !string.IsNullOrWhiteSpace(await ObtenerApiKeyAsync(cancellationToken));

    private Task<string> ObtenerApiKeyAsync(CancellationToken cancellationToken) =>
        parametros.GetTextoAsync(ParametrosSistemaKeys.GeminiApiKey, string.Empty, cancellationToken);

    public async Task<InterpretacionVozDto> InterpretarAsync(
        string transcripcion,
        CatalogoVozDto catalogo,
        DateOnly hoy,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await ObtenerApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
            return InterpretacionVozDto.Vacia;

        var resultado = await cliente.GenerarJsonAsync(
            apiKey,
            _settings.Model,
            _settings.MaxTokens,
            ConstruirInstrucciones(catalogo, hoy),
            transcripcion,
            Esquema(),
            cancellationToken);

        // Un rechazo (filtro de seguridad, prompt bloqueado) no trae contenido util. Se
        // trata como "no entendi": el formulario queda como estaba y el usuario completa a
        // mano.
        if (resultado.Rechazado)
        {
            logger.LogWarning("Gemini rechazo interpretar el dictado.");
            return InterpretacionVozDto.Vacia;
        }

        return string.IsNullOrWhiteSpace(resultado.Json)
            ? InterpretacionVozDto.Vacia
            : Deserializar(resultado.Json);
    }

    private InterpretacionVozDto Deserializar(string json)
    {
        try
        {
            var crudo = JsonSerializer.Deserialize<RespuestaCruda>(json);
            if (crudo is null) return InterpretacionVozDto.Vacia;

            return new InterpretacionVozDto(
                Fecha:        Fecha(crudo.fecha),
                HoraEntrada1: Hora(crudo.hora_entrada_1),
                HoraSalida1:  Hora(crudo.hora_salida_1),
                HoraEntrada2: Hora(crudo.hora_entrada_2),
                HoraSalida2:  Hora(crudo.hora_salida_2),
                HoraEntrada3: Hora(crudo.hora_entrada_3),
                HoraSalida3:  Hora(crudo.hora_salida_3),
                Cliente:      Texto(crudo.cliente),
                Proyecto:     Texto(crudo.proyecto),
                Modalidad:    Texto(crudo.modalidad),
                Recurso:      Texto(crudo.recurso),
                Lugar:        Texto(crudo.lugar),
                Descripcion:  Texto(crudo.descripcion));
        }
        catch (JsonException ex)
        {
            // El dictado no vale un 500: se responde vacio y el usuario llena a mano.
            logger.LogWarning(ex, "No se pudo leer la respuesta de Gemini para el dictado.");
            return InterpretacionVozDto.Vacia;
        }
    }

    private static string? Texto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static DateOnly? Fecha(string? valor) =>
        DateOnly.TryParse(valor, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : null;

    private static TimeOnly? Hora(string? valor) =>
        TimeOnly.TryParse(valor, System.Globalization.CultureInfo.InvariantCulture, out var h) ? h : null;

    private static string ConstruirInstrucciones(CatalogoVozDto catalogo, DateOnly hoy)
    {
        var proyectos = string.Join("\n", catalogo.ProyectosPorCliente
            .Select(par => $"  - {par.Key}: {string.Join(", ", par.Value)}"));

        return $"""
            Extraes los datos de un registro de horas a partir de lo que un empleado dicta
            en voz alta, en español de Colombia. Devuelves unicamente el JSON del esquema.

            Hoy es {hoy:yyyy-MM-dd} ({Dia(hoy)}).

            REGLAS
            - Un campo que el empleado no menciona va en null. No adivines ni completes con
              lo que parezca probable: el formulario conserva lo que ya tenia.
            - Cliente, proyecto, modalidad, recurso y lugar SOLO pueden ser un valor exacto
              de las listas de abajo. Si lo dictado no coincide con ninguno, va null.
            - El proyecto debe pertenecer al cliente elegido.
            - Las horas van en formato 24 horas "HH:mm". Interpreta el contexto de la jornada:
              "entro a las ocho" es 08:00 y "salgo a las seis" es 18:00.
            - Si dicta un solo tramo, usa entrada/salida 1. El segundo tramo es el de despues
              del almuerzo. El tercero solo si menciona un tercer bloque.
            - Las fechas van "yyyy-MM-dd". Resuelve expresiones relativas ("ayer", "el lunes
              pasado") contra la fecha de hoy. Nunca devuelvas una fecha futura.
            - La descripcion es el texto de la tarea, limpio y sin muletillas. Si no describe
              ninguna tarea, va null.

            CLIENTES Y SUS PROYECTOS
            {(proyectos.Length > 0 ? proyectos : "  (sin proyectos activos)")}

            MODALIDADES: {Lista(catalogo.Modalidades)}
            RECURSOS: {Lista(catalogo.Recursos)}
            LUGARES: {Lista(catalogo.Lugares)}
            """;
    }

    private static string Lista(IReadOnlyList<string> valores) =>
        valores.Count > 0 ? string.Join(", ", valores) : "(ninguno)";

    private static string Dia(DateOnly fecha) =>
        fecha.ToString("dddd", new System.Globalization.CultureInfo("es-CO"));

    private static object Esquema()
    {
        string[] campos =
        [
            "fecha",
            "hora_entrada_1", "hora_salida_1",
            "hora_entrada_2", "hora_salida_2",
            "hora_entrada_3", "hora_salida_3",
            "cliente", "proyecto", "modalidad", "recurso", "lugar", "descripcion"
        ];

        // Todos los campos son nullable: obliga al modelo a pronunciarse sobre cada uno en
        // vez de omitir los que no encontro.
        var propiedades = campos.ToDictionary(
            campo => campo,
            object (_) => new { type = "STRING", nullable = true });

        return new
        {
            type = "OBJECT",
            properties = propiedades,
            required = campos
        };
    }

    /// <summary>Espejo del JSON del modelo: nombres tal cual el esquema, todo texto o null.</summary>
    private sealed record RespuestaCruda(
        string? fecha,
        string? hora_entrada_1, string? hora_salida_1,
        string? hora_entrada_2, string? hora_salida_2,
        string? hora_entrada_3, string? hora_salida_3,
        string? cliente, string? proyecto, string? modalidad,
        string? recurso, string? lugar, string? descripcion);
}
