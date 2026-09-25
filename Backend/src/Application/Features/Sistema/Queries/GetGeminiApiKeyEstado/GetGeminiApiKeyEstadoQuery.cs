using KPG.Timesheet.Application.Common.Interfaces;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetGeminiApiKeyEstado;

/// <summary>
/// Estado de la API key de Gemini, sin exponer el valor completo: la pantalla de
/// administracion solo necesita saber si hay una configurada y ver los ultimos caracteres
/// para confirmar cual es, no volver a mostrarla entera.
/// </summary>
public record GetGeminiApiKeyEstadoQuery : IRequest<GeminiApiKeyEstadoDto>;

public record GeminiApiKeyEstadoDto(bool Configurada, string? Mascara);

public class GetGeminiApiKeyEstadoQueryHandler(IParametrosSistemaService parametros)
    : IRequestHandler<GetGeminiApiKeyEstadoQuery, GeminiApiKeyEstadoDto>
{
    public async Task<GeminiApiKeyEstadoDto> Handle(
        GetGeminiApiKeyEstadoQuery request, CancellationToken cancellationToken)
    {
        var valor = await parametros.GetTextoAsync(
            ParametrosSistemaKeys.GeminiApiKey, string.Empty, cancellationToken);

        if (string.IsNullOrWhiteSpace(valor))
            return new GeminiApiKeyEstadoDto(false, null);

        var ultimos = valor.Length > 4 ? valor[^4..] : valor;
        return new GeminiApiKeyEstadoDto(true, $"••••{ultimos}");
    }
}
