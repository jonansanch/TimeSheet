using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using ReglaEntity = KPG.Timesheet.Domain.Entities.ReglaVentanaRetroactividad;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using FluentValidation.Results;

namespace KPG.Timesheet.Application.Features.Sistema.ReglasVentana;

public record ReglaVentanaDto(
    int Id,
    string? UserId,
    string? Rol,
    int Dias,
    bool Activo);

// ── Listar ───────────────────────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record GetReglasVentanaQuery : IRequest<IReadOnlyList<ReglaVentanaDto>>;

public class GetReglasVentanaQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReglasVentanaQuery, IReadOnlyList<ReglaVentanaDto>>
{
    public async Task<IReadOnlyList<ReglaVentanaDto>> Handle(
        GetReglasVentanaQuery request,
        CancellationToken cancellationToken)
    {
        // Las de rol primero: son las que mas gente afectan.
        return await context.ReglasVentanaRetroactividad
            .OrderBy(r => r.UserId != null)
            .ThenBy(r => r.Rol)
            .Select(r => new ReglaVentanaDto(r.Id, r.UserId, r.Rol, r.Dias, r.Activo))
            .ToListAsync(cancellationToken);
    }
}

// ── Crear / actualizar ───────────────────────────────────────────────────────

/// <summary>
/// Alta o edicion de una excepcion. Exactamente uno de <paramref name="UserId"/> o
/// <paramref name="Rol"/> debe venir informado.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record GuardarReglaVentanaCommand(
    int? Id,
    string? UserId,
    string? Rol,
    int Dias) : IRequest<ReglaVentanaDto>;

public class GuardarReglaVentanaCommandValidator : AbstractValidator<GuardarReglaVentanaCommand>
{
    private static readonly string[] RolesValidos =
        [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];

    public GuardarReglaVentanaCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.UserId) ^ string.IsNullOrWhiteSpace(x.Rol))
            .WithMessage("La regla debe aplicar a una persona o a un rol, no a ambos ni a ninguno.");

        RuleFor(x => x.Rol)
            .Must(rol => RolesValidos.Contains(rol))
            .When(x => !string.IsNullOrWhiteSpace(x.Rol))
            .WithMessage("El rol seleccionado no es valido.");

        RuleFor(x => x.Dias)
            .InclusiveBetween(0, ReglaEntity.MaxDias)
            .WithMessage($"Los dias deben estar entre 0 y {ReglaEntity.MaxDias}.");
    }
}

public class GuardarReglaVentanaCommandHandler(
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<GuardarReglaVentanaCommand, ReglaVentanaDto>
{
    public async Task<ReglaVentanaDto> Handle(
        GuardarReglaVentanaCommand request,
        CancellationToken cancellationToken)
    {
        var userId = string.IsNullOrWhiteSpace(request.UserId) ? null : request.UserId.Trim();
        var rol    = string.IsNullOrWhiteSpace(request.Rol)    ? null : request.Rol.Trim();

        // La BD lo impide con indices unicos filtrados, pero aqui el mensaje es entendible.
        var duplicada = await context.ReglasVentanaRetroactividad.AnyAsync(
            r => ((userId != null && r.UserId == userId) || (rol != null && r.Rol == rol))
              && (request.Id == null || r.Id != request.Id),
            cancellationToken);

        if (duplicada)
        {
            throw new ValidationException([
                new ValidationFailure(
                    userId is not null ? nameof(request.UserId) : nameof(request.Rol),
                    userId is not null
                        ? "Esa persona ya tiene una regla de ventana."
                        : "Ese rol ya tiene una regla de ventana.")]);
        }

        ReglaEntity regla;

        if (request.Id is null)
        {
            regla = userId is not null
                ? ReglaEntity.ParaUsuario(userId, request.Dias)
                : ReglaEntity.ParaRol(rol!, request.Dias);
            context.ReglasVentanaRetroactividad.Add(regla);
        }
        else
        {
            regla = await context.ReglasVentanaRetroactividad
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException($"Regla de ventana con id '{request.Id}' no fue encontrada.");

            // Solo cambian los dias: persona y rol son la identidad de la regla.
            regla.CambiarDias(request.Dias);
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioVentanaRetroactividad,
            actor.Id ?? "system", null,
            "ReglasVentanaRetroactividad", request.Id?.ToString(),
            new { userId, rol, request.Dias },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ReglaVentanaDto(regla.Id, regla.UserId, regla.Rol, regla.Dias, regla.Activo);
    }
}

// ── Activar / desactivar ─────────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record ToggleReglaVentanaCommand(int Id) : IRequest<ReglaVentanaDto>;

public class ToggleReglaVentanaCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleReglaVentanaCommand, ReglaVentanaDto>
{
    public async Task<ReglaVentanaDto> Handle(
        ToggleReglaVentanaCommand request,
        CancellationToken cancellationToken)
    {
        var regla = await context.ReglasVentanaRetroactividad
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Regla de ventana con id '{request.Id}' no fue encontrada.");

        if (regla.Activo) regla.Desactivar(); else regla.Activar();
        await context.SaveChangesAsync(cancellationToken);

        return new ReglaVentanaDto(regla.Id, regla.UserId, regla.Rol, regla.Dias, regla.Activo);
    }
}
