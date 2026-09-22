using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Commands.UpdateLogoReportes;

/// <summary>Guarda o quita el logo que se imprime en los reportes. Nulo o vacio lo quita.</summary>
public record UpdateLogoReportesCommand(string? ImagenDataUri) : IRequest;

public class UpdateLogoReportesCommandValidator : AbstractValidator<UpdateLogoReportesCommand>
{
    /// <summary>~1.4 MB en base64, equivalente a ~1 MB de imagen original.</summary>
    public const int MaxLargoDataUri = 1_900_000;

    public UpdateLogoReportesCommandValidator()
    {
        RuleFor(x => x.ImagenDataUri)
            .Must(EsDataUriDeImagenValida)
            .When(x => !string.IsNullOrWhiteSpace(x.ImagenDataUri))
            .WithMessage("La imagen debe ser PNG o JPG y no puede superar 1 MB.");
    }

    private static bool EsDataUriDeImagenValida(string? dataUri) =>
        dataUri!.Length <= MaxLargoDataUri &&
        (dataUri.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase) ||
         dataUri.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase) ||
         dataUri.StartsWith("data:image/jpg;base64,", StringComparison.OrdinalIgnoreCase));
}

public class UpdateLogoReportesCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateLogoReportesCommand>
{
    public async Task Handle(UpdateLogoReportesCommand request, CancellationToken cancellationToken)
    {
        var valor = request.ImagenDataUri?.Trim() ?? string.Empty;

        var param = await context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.LogoReportes, cancellationToken);

        if (param is not null)
        {
            param.Valor = valor;
        }
        else
        {
            context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.LogoReportes,
                Valor = valor
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
