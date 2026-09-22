using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using ReporteEntity = KPG.Timesheet.Domain.Entities.ReporteUsuario;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using FluentValidation.Results;

namespace KPG.Timesheet.Application.Features.ReportesUsuario;

public record ReporteUsuarioDto(
    int Id,
    string UserId,
    TipoReporte Tipo,
    string Titulo,
    string Descripcion,
    EstadoReporte Estado,
    string? ComentarioRespuesta,
    string? RespondidoPorUserId,
    DateTimeOffset Created);

// ── Crear ────────────────────────────────────────────────────────────────────

/// <summary>Cualquier persona autenticada puede reportar una falla o pedir una mejora.</summary>
[Authorize(Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record CreateReporteUsuarioCommand(
    TipoReporte Tipo,
    string Titulo,
    string Descripcion) : IRequest<ReporteUsuarioDto>;

public class CreateReporteUsuarioCommandValidator : AbstractValidator<CreateReporteUsuarioCommand>
{
    public CreateReporteUsuarioCommandValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(2000);
    }
}

public class CreateReporteUsuarioCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IBitacoraService bitacora)
    : IRequestHandler<CreateReporteUsuarioCommand, ReporteUsuarioDto>
{
    public async Task<ReporteUsuarioDto> Handle(
        CreateReporteUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var reporte = new ReporteEntity(userId, request.Tipo, request.Titulo, request.Descripcion);
        context.ReportesUsuario.Add(reporte);

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ReporteUsuarioCreado,
            userId, user.Email,
            "ReportesUsuario", null,
            new { request.Tipo, request.Titulo },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return ToDto(reporte);
    }

    private static ReporteUsuarioDto ToDto(ReporteEntity r) => new(
        r.Id, r.UserId, r.Tipo, r.Titulo, r.Descripcion, r.Estado,
        r.ComentarioRespuesta, r.RespondidoPorUserId, r.Created);
}

// ── Mis reportes ─────────────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetMisReportesQuery : IRequest<IReadOnlyList<ReporteUsuarioDto>>;

public class GetMisReportesQueryHandler(IApplicationDbContext context, IUser user)
    : IRequestHandler<GetMisReportesQuery, IReadOnlyList<ReporteUsuarioDto>>
{
    public async Task<IReadOnlyList<ReporteUsuarioDto>> Handle(
        GetMisReportesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        return await context.ReportesUsuario
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Created)
            .Select(r => new ReporteUsuarioDto(
                r.Id, r.UserId, r.Tipo, r.Titulo, r.Descripcion, r.Estado,
                r.ComentarioRespuesta, r.RespondidoPorUserId, r.Created))
            .ToListAsync(cancellationToken);
    }
}

// ── Listar todos (Admin) ────────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record GetTodosReportesQuery : IRequest<IReadOnlyList<ReporteUsuarioDto>>;

public class GetTodosReportesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTodosReportesQuery, IReadOnlyList<ReporteUsuarioDto>>
{
    public async Task<IReadOnlyList<ReporteUsuarioDto>> Handle(
        GetTodosReportesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.ReportesUsuario
            .OrderByDescending(r => r.Created)
            .Select(r => new ReporteUsuarioDto(
                r.Id, r.UserId, r.Tipo, r.Titulo, r.Descripcion, r.Estado,
                r.ComentarioRespuesta, r.RespondidoPorUserId, r.Created))
            .ToListAsync(cancellationToken);
    }
}

// ── Cambiar estado (Admin) ──────────────────────────────────────────────────

[Authorize(Roles = Roles.Admin)]
public record CambiarEstadoReporteCommand(
    int Id,
    EstadoReporte NuevoEstado,
    string? Comentario) : IRequest<ReporteUsuarioDto>;

public class CambiarEstadoReporteCommandValidator : AbstractValidator<CambiarEstadoReporteCommand>
{
    public CambiarEstadoReporteCommandValidator()
    {
        RuleFor(x => x.NuevoEstado).IsInEnum();

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .WithMessage("El rechazo requiere un comentario que explique el motivo.")
            .When(x => x.NuevoEstado == EstadoReporte.Rechazado);

        RuleFor(x => x.Comentario).MaximumLength(2000);
    }
}

public class CambiarEstadoReporteCommandHandler(
    IApplicationDbContext context,
    IUser actor,
    IBitacoraService bitacora)
    : IRequestHandler<CambiarEstadoReporteCommand, ReporteUsuarioDto>
{
    public async Task<ReporteUsuarioDto> Handle(
        CambiarEstadoReporteCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var reporte = await context.ReportesUsuario
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Reporte con id '{request.Id}' no fue encontrado.");

        try
        {
            switch (request.NuevoEstado)
            {
                case EstadoReporte.EnRevision:
                    reporte.MarcarEnRevision(actorId, request.Comentario);
                    break;
                case EstadoReporte.Resuelto:
                    reporte.Resolver(actorId, request.Comentario);
                    break;
                case EstadoReporte.Rechazado:
                    reporte.Rechazar(actorId, request.Comentario!);
                    break;
                default:
                    throw new ValidationException([
                        new ValidationFailure(nameof(request.NuevoEstado), "Estado destino no valido.")]);
            }
        }
        catch (Domain.Exceptions.DomainRuleException ex)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(request.NuevoEstado), ex.Message)]);
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ReporteUsuarioCambioEstado,
            actorId, actor.Email,
            "ReportesUsuario", request.Id.ToString(),
            new { request.NuevoEstado, request.Comentario },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new ReporteUsuarioDto(
            reporte.Id, reporte.UserId, reporte.Tipo, reporte.Titulo, reporte.Descripcion, reporte.Estado,
            reporte.ComentarioRespuesta, reporte.RespondidoPorUserId, reporte.Created);
    }
}
