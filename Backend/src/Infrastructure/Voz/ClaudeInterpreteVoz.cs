using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPG.Timesheet.Infrastructure.Voz;

public class AnthropicSettings
{
    /// <summary>
    /// Vacia en appsettings a proposito: se inyecta por variable de entorno
    /// (ANTHROPIC_API_KEY o Anthropic__ApiKey). Nunca se commitea una key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-opus-5";

    /// <summary>Un dictado es una frase corta: no hace falta pensar mucho ni escribir mucho.</summary>
    public int MaxTokens { get; set; } = 1024;
}

/// <summary>
/// Interpreta el dictado con Claude usando salida estructurada: el modelo devuelve
/// directamente el JSON con la forma que espera el formulario, sin texto alrededor que
/// haya que recortar.
///
/// <para>
/// No inventa valores de catalogo: el prompt le pasa los clientes, proyectos, modalidades,
/// recursos y lugares reales y le exige elegir de esas listas o dejar el campo nulo.
/// </para>
/// </summary>
public class ClaudeInterpreteVoz(
    IOptions<AnthropicSettings> settings,
    ILogger<ClaudeInterpreteVoz> logger) : IInterpreteVoz
{
    private readonly AnthropicSettings _settings = settings.Value;

    public bool Disponible => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    public async Task<InterpretacionVozDto> InterpretarAsync(
        string transcripcion,
        CatalogoVozDto catalogo,
        DateOnly hoy,
        CancellationToken cancellationToken = default)
    {
        if (!Disponible)
            return InterpretacionVozDto.Vacia;

        var client = new AnthropicClient { ApiKey = _settings.ApiKey };

        var respuesta = await client.Messages.Create(new MessageCreateParams
        {
            Model      = _settings.Model,
            MaxTokens  = _settings.MaxTokens,
            System     = ConstruirInstrucciones(catalogo, hoy),
            // Extraccion corta y acotada: no necesita el esfuerzo alto por defecto.
            OutputConfig = new OutputConfig
            {
                Effort = Effort.Low,
                Format = new JsonOutputFormat { Schema = Esquema() }
            },
            Messages = [new() { Role = Role.User, Content = transcripcion }]
        }, cancellationToken: cancellationToken);

        // Una negativa del modelo no trae contenido util. Se trata como "no entendi":
        // el formulario queda como estaba y el usuario completa a mano.
        if (respuesta.StopReason == StopReason.Refusal)
        {
            logger.LogWarning("Claude rechazo interpretar el dictado: {Detalle}", respuesta.StopDetails);
            return InterpretacionVozDto.Vacia;
        }

        // El contenido es una union de bloques; solo interesa el de texto, que con salida
        // estructurada trae el JSON completo.
        var json = respuesta.Content
            .Select(bloque => bloque.TryPickText(out var texto) ? texto.Text : null)
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

        return string.IsNullOrWhiteSpace(json)
            ? InterpretacionVozDto.Vacia
            : Deserializar(json);
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
            logger.LogWarning(ex, "No se pudo leer la respuesta de Claude para el dictado.");
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

    private static Dictionary<string, JsonElement> Esquema()
    {
        string[] campos =
        [
            "fecha",
            "hora_entrada_1", "hora_salida_1",
            "hora_entrada_2", "hora_salida_2",
            "hora_entrada_3", "hora_salida_3",
            "cliente", "proyecto", "modalidad", "recurso", "lugar", "descripcion"
        ];

        // Todos los campos son requeridos y nullables: obliga al modelo a pronunciarse
        // sobre cada uno en vez de omitir los que no encontro.
        var propiedades = campos.ToDictionary(
            campo => campo,
            _ => new { type = new[] { "string", "null" } });

        return new Dictionary<string, JsonElement>
        {
            ["type"]                 = JsonSerializer.SerializeToElement("object"),
            ["properties"]           = JsonSerializer.SerializeToElement(propiedades),
            ["required"]             = JsonSerializer.SerializeToElement(campos),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false)
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
