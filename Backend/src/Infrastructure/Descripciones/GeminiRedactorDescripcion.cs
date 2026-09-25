using System.Text.Json;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Infrastructure.Ia;
using KPG.Timesheet.Infrastructure.Voz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Infrastructure.Descripciones;

public class RedactorDescripcionSettings
{
    /// <summary>
    /// Modelo aparte del dictado por voz (<see cref="GeminiSettings"/>): reescribir
    /// una frase corta con un formato fijo no necesita el modelo mas grande.
    /// </summary>
    public string Model { get; set; } = "gemini-flash-lite-latest";

    public int MaxTokens { get; set; } = 512;

    /// <summary>
    /// Tope de usos por usuario por dia (ver Docs/plan-calidad-descripciones.md). Cada
    /// llamada tiene costo; esto evita que alguien lo use como corrector de estilo general.
    /// </summary>
    public int LimiteDiarioPorUsuario { get; set; } = 20;
}

/// <summary>
/// Reescribe la descripcion con Gemini, con salida estructurada, reutilizando la API key de
/// <see cref="Domain.Constants.ParametrosSistema.GeminiApiKey"/> (misma cuenta que el dictado
/// de voz) pero con su propio modelo y limites.
/// </summary>
public class GeminiRedactorDescripcion(
    GeminiApiClient cliente,
    IParametrosSistemaService parametros,
    IOptions<RedactorDescripcionSettings> settings,
    ILogger<GeminiRedactorDescripcion> logger) : IRedactorDescripcion
{
    private readonly RedactorDescripcionSettings _settings = settings.Value;

    public async Task<bool> DisponibleAsync(CancellationToken cancellationToken = default) =>
        !string.IsNullOrWhiteSpace(await ObtenerApiKeyAsync(cancellationToken));

    private Task<string> ObtenerApiKeyAsync(CancellationToken cancellationToken) =>
        parametros.GetTextoAsync(ParametrosSistemaKeys.GeminiApiKey, string.Empty, cancellationToken);

    public int LimiteDiarioPorUsuario => _settings.LimiteDiarioPorUsuario;

    public async Task<PropuestaDescripcionDto> MejorarAsync(
        string texto,
        string? nombreCliente,
        string? nombreProyecto,
        IReadOnlyList<string> hallazgosActuales,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await ObtenerApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(texto))
            return PropuestaDescripcionDto.Vacia;

        var resultado = await cliente.GenerarJsonAsync(
            apiKey,
            _settings.Model,
            _settings.MaxTokens,
            ConstruirInstrucciones(nombreCliente, nombreProyecto, hallazgosActuales),
            texto,
            Esquema(),
            cancellationToken);

        // Igual que con el dictado: un rechazo no trae contenido util. Se trata como "no se
        // pudo mejorar" y el usuario sigue con el texto que ya tenia.
        if (resultado.Rechazado)
        {
            logger.LogWarning("Gemini rechazo mejorar la descripcion.");
            return PropuestaDescripcionDto.Vacia;
        }

        return string.IsNullOrWhiteSpace(resultado.Json) ? PropuestaDescripcionDto.Vacia : Deserializar(resultado.Json);
    }

    private PropuestaDescripcionDto Deserializar(string json)
    {
        try
        {
            var crudo = JsonSerializer.Deserialize<RespuestaCruda>(json);
            if (crudo is null || string.IsNullOrWhiteSpace(crudo.propuesta))
                return PropuestaDescripcionDto.Vacia;

            return new PropuestaDescripcionDto(
                crudo.propuesta.Trim(),
                (crudo.cambios ?? []).Where(c => !string.IsNullOrWhiteSpace(c)).ToList());
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "No se pudo leer la respuesta de Gemini para mejorar la descripcion.");
            return PropuestaDescripcionDto.Vacia;
        }
    }

    private static string ConstruirInstrucciones(
        string? nombreCliente, string? nombreProyecto, IReadOnlyList<string> hallazgosActuales)
    {
        var contexto = nombreProyecto is null
            ? "No se conoce el proyecto de este registro."
            : $"Cliente: {nombreCliente ?? "(sin cliente)"}. Proyecto: {nombreProyecto}.";

        var hallazgos = hallazgosActuales.Count > 0
            ? string.Join("\n", hallazgosActuales.Select(h => $"  - {h}"))
            : "  (ninguno detectado por las reglas automaticas)";

        return $"""
            Reescribes la descripcion de una tarea de un registro de horas, en español,
            siguiendo esta guia interna de KPG Inc. Devuelves unicamente el JSON del esquema.

            FORMATO RECOMENDADO
            [App/Módulo] – [Acción técnica] – [Elemento afectado] – [Resultado esperado]

            EJEMPLOS DE BUENAS DESCRIPCIONES
            - CIMA WebClient – Ajuste en módulo de evidencias para solicitudes de cancelación.
            - ACUDEN Digital – Optimización aplicada al someter solicitud.
            - API – Validación de SPS en aprobar y rechazar documentos.

            PRINCIPIOS
            - Describe la tarea, no una accion generica. Se especifico y concreto.
            - Tono neutro y ejecutivo, sin muletillas ni primera persona.
            - Enfocate en el resultado o proposito, no solo en la accion.

            CONTEXTO DEL REGISTRO
            {contexto}

            PROBLEMAS YA DETECTADOS EN EL TEXTO ORIGINAL (por reglas automaticas)
            {hallazgos}

            REGLAS INQUEBRANTABLES
            - No inventes hechos que el usuario no haya mencionado: ni sistemas, ni personas,
              ni resultados, ni tareas. Solo puedes reordenar, precisar y aplicar el formato
              a lo que el texto original ya dice.
            - Si al texto original le falta un dato que el formato pide (por ejemplo, el
              elemento afectado), deja un marcador como "[elemento afectado]" en su lugar en
              vez de adivinarlo.
            - Si el texto original ya cumple la guia, tu propuesta puede ser igual o casi
              igual: no cambies por cambiar.
            - "cambios" es una lista corta (maximo 3) de frases breves explicando que se
              ajusto, por ejemplo "Se agrego el modulo afectado" o "Se aplico el formato
              App - Accion - Elemento - Resultado". Vacia si no hubo cambios de fondo.
            """;
    }

    private static object Esquema() => new
    {
        type = "OBJECT",
        properties = new
        {
            propuesta = new { type = "STRING" },
            cambios = new { type = "ARRAY", items = new { type = "STRING" } }
        },
        required = new[] { "propuesta", "cambios" }
    };

    private sealed record RespuestaCruda(string? propuesta, List<string>? cambios);
}
