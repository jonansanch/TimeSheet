using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Commands.UpdateGeminiApiKey;

/// <summary>Reemplaza la API key de Gemini usada por el dictado de voz y "mejorar redaccion".</summary>
/// <param name="ApiKey">Vacio o null la quita: ambas funciones vuelven a quedar "no disponibles".</param>
public record UpdateGeminiApiKeyCommand(string? ApiKey) : IRequest;

public class UpdateGeminiApiKeyCommandValidator : AbstractValidator<UpdateGeminiApiKeyCommand>
{
    public UpdateGeminiApiKeyCommandValidator()
    {
        RuleFor(x => x.ApiKey)
            .MaximumLength(512).WithMessage("La API key no puede superar 512 caracteres.");
    }
}

public class UpdateGeminiApiKeyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateGeminiApiKeyCommand>
{
    public async Task Handle(UpdateGeminiApiKeyCommand request, CancellationToken cancellationToken)
    {
        var valor = request.ApiKey?.Trim() ?? string.Empty;

        var param = await context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.GeminiApiKey, cancellationToken);

        if (param is not null)
        {
            param.Valor = valor;
        }
        else
        {
            context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.GeminiApiKey,
                Valor = valor
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
