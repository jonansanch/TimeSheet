using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using RestriccionEntity = KPG.Timesheet.Domain.Entities.ParametroRestriccionDia;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using FluentValidation.Results;

namespace KPG.Timesheet.Application.Features.Sistema.RestriccionesDia;

public record RestriccionDiaDto(
    int Id,
    DayOfWeek DiaDelaSemana,
    string? UserId,
    string? Rol,
    bool Activo);

// ── Listar ───────────────────────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record GetRestriccionesDiaQuery : IRequest<IReadOnlyList<RestriccionDiaDto>>;

public class GetRestriccionesDiaQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetRestriccionesDiaQuery, IReadOnlyList<RestriccionDiaDto>>
{
    public async Task<IReadOnlyList<RestriccionDiaDto>> Handle(
        GetRestriccionesDiaQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ParametrosRestriccionDia
            .OrderBy(r => r.DiaDelaSemana)
            .ThenBy(r => r.UserId != null)
            .ThenBy(r => r.Rol)
            .Select(r => new RestriccionDiaDto(r.Id, r.DiaDelaSemana, r.UserId, r.Rol, r.Activo))
            .ToListAsync(cancellationToken);
    }
}

// ── Crear ────────────────────────────────────────────────────────────────────

/// <summary>Exactamente uno de <paramref name="UserId"/> o <paramref name="Rol"/> debe venir informado.</summary>
[Authorize(Roles = Roles.Admin)]
public record GuardarRestriccionDiaCommand(
    DayOfWeek DiaDelaSemana,
    string? UserId,
    string? Rol) : IRequest<RestriccionDiaDto>;

public class GuardarRestriccionDiaCommandValidator : AbstractValidator<GuardarRestriccionDiaCommand>
{
    private static readonly string[] RolesValidos =
        [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];

    public GuardarRestriccionDiaCommandValidator()
    {
        RuleFor(x => x.DiaDelaSemana).IsInEnum();

        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.UserId) ^ string.IsNullOrWhiteSpace(x.Rol))
            .WithMessage("La restriccion debe aplicar a una persona o a un rol, no a ambos ni a ninguno.");

        RuleFor(x => x.Rol)
            .Must(rol => RolesValidos.Contains(rol))
            .When(x => !string.IsNullOrWhiteSpace(x.Rol))
            .WithMessage("El rol seleccionado no es valido.");
    }
}

public class GuardarRestriccionDiaCommandHandler(
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<GuardarRestriccionDiaCommand, RestriccionDiaDto>
{
    public async Task<RestriccionDiaDto> Handle(
        GuardarRestriccionDiaCommand request,
        CancellationToken cancellationToken)
    {
        var userId = string.IsNullOrWhiteSpace(request.UserId) ? null : request.UserId.Trim();
        var rol    = string.IsNullOrWhiteSpace(request.Rol)    ? null : request.Rol.Trim();

        // La BD lo impide con indices unicos filtrados, pero aqui el mensaje es entendible.
        var duplicada = await context.ParametrosRestriccionDia.AnyAsync(
            r => r.DiaDelaSemana == request.DiaDelaSemana
              && ((userId != null && r.UserId == userId) || (rol != null && r.Rol == rol)),
            cancellationToken);

        if (duplicada)
        {
            throw new ValidationException([
                new ValidationFailure(
                    userId is not null ? nameof(request.UserId) : nameof(request.Rol),
                    userId is not null
                        ? "Esa persona ya tiene una restriccion para ese dia."
                        : "Ese rol ya tiene una restriccion para ese dia.")]);
        }

        var restriccion = userId is not null
            ? RestriccionEntity.ParaUsuario(request.DiaDelaSemana, userId)
            : RestriccionEntity.ParaRol(request.DiaDelaSemana, rol!);

        context.ParametrosRestriccionDia.Add(restriccion);

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioRestriccionDia,
            actor.Id ?? "system", null,
            "ParametrosRestriccionDia", null,
            new { request.DiaDelaSemana, userId, rol },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new RestriccionDiaDto(
            restriccion.Id, restriccion.DiaDelaSemana, restriccion.UserId, restriccion.Rol, restriccion.Activo);
    }
}

// ── Activar / desactivar ─────────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record ToggleRestriccionDiaCommand(int Id) : IRequest<RestriccionDiaDto>;

public class ToggleRestriccionDiaCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleRestriccionDiaCommand, RestriccionDiaDto>
{
    public async Task<RestriccionDiaDto> Handle(
        ToggleRestriccionDiaCommand request,
        CancellationToken cancellationToken)
    {
        var restriccion = await context.ParametrosRestriccionDia
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Restriccion de dia con id '{request.Id}' no fue encontrada.");

        if (restriccion.Activo) restriccion.Desactivar(); else restriccion.Activar();
        await context.SaveChangesAsync(cancellationToken);

        return new RestriccionDiaDto(
            restriccion.Id, restriccion.DiaDelaSemana, restriccion.UserId, restriccion.Rol, restriccion.Activo);
    }
}
