using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Aprobaciones;

public class GetPendientesAprobacionQueryHandler(
    IAprobacionesRepository repository,
    IUser user)
    : IRequestHandler<GetPendientesAprobacionQuery, PendientesAprobacionResponse>
{
    public Task<PendientesAprobacionResponse> Handle(
        GetPendientesAprobacionQuery request,
        CancellationToken cancellationToken)
    {
        var revisorId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        return repository.GetPendientesAsync(revisorId, request, cancellationToken);
    }
}

public class GetPendientesAprobacionQueryValidator : AbstractValidator<GetPendientesAprobacionQuery>
{
    public GetPendientesAprobacionQueryValidator()
    {
        RuleFor(x => x.Hasta)
            .GreaterThanOrEqualTo(x => x.Desde)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");

        // Un rango abierto traeria el historico entero a la pantalla de revision.
        RuleFor(x => x)
            .Must(x => x.Hasta.DayNumber - x.Desde.DayNumber <= 366)
            .WithMessage("El rango no puede superar un ano.");
    }
}
