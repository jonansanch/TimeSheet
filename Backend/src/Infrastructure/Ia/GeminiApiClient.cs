using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Infrastructure.Ia;

/// <summary>
/// Resultado de una llamada a Gemini con salida estructurada.
/// </summary>
/// <param name="Json">El JSON devuelto por el modelo, o null si no hubo contenido util.</param>
/// <param name="Rechazado">
/// True cuando el modelo (o el filtro de seguridad de Gemini) no genero contenido, por
/// ejemplo por <c>SAFETY</c> o por bloqueo del prompt. Se distingue de un error de red para
/// que quien llama pueda loguear cada caso distinto.
/// </param>
public record GeminiResultado(string? Json, bool Rechazado);

/// <summary>
/// Cliente minimo para la API de Gemini (generateContent con salida JSON), compartido por el
/// dictado de voz y "mejorar redaccion". No usa el SDK oficial de Google: es una sola llamada
/// REST con esquema, y evita arrastrar una dependencia mas por eso.
/// </summary>
public class GeminiApiClient(HttpClient http, ILogger<GeminiApiClient> logger)
{
    public async Task<GeminiResultado> GenerarJsonAsync(
        string apiKey,
        string modelo,
        int maxOutputTokens,
        string instrucciones,
        string texto,
        object esquema,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = instrucciones } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = texto } } } },
            generationConfig = new
            {
                maxOutputTokens,
                responseMimeType = "application/json",
                responseSchema = esquema
            }
        };

        using var response = await http.PostAsJsonAsync(
            $"v1beta/models/{modelo}:generateContent?key={Uri.EscapeDataString(apiKey)}",
            body,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Gemini devolvio {StatusCode} para el modelo {Modelo}: {Detalle}",
                (int)response.StatusCode, modelo, detalle);
            return new GeminiResultado(null, Rechazado: false);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var documento = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var raiz = documento.RootElement;

        if (!raiz.TryGetProperty("candidates", out var candidatos) || candidatos.GetArrayLength() == 0)
            return new GeminiResultado(null, Rechazado: true);

        var candidato = candidatos[0];
        var finishReason = candidato.TryGetProperty("finishReason", out var fr) ? fr.GetString() : null;
        if (finishReason is "SAFETY" or "RECITATION" or "PROHIBITED_CONTENT" or "BLOCKLIST")
            return new GeminiResultado(null, Rechazado: true);

        var textoRespuesta = candidato
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return new GeminiResultado(textoRespuesta, Rechazado: false);
    }
}
