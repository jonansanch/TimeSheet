using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Aprobaciones;

public record EmpleadoRevisableDto(string UserId, string Nombre);

/// <summary>
/// Los empleados que le toca revisar al usuario autenticado. Alimenta el filtro de la
/// pantalla de aprobaciones, que antes se llenaba con la lista completa de usuarios.
/// </summary>
[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetEmpleadosRevisablesQuery : IRequest<IReadOnlyList<EmpleadoRevisableDto>>;

public class GetEmpleadosRevisablesQueryHandler(
    IAprobacionesRepository repositorio,
    IUser usuario)
    : IRequestHandler<GetEmpleadosRevisablesQuery, IReadOnlyList<EmpleadoRevisableDto>>
{
    public async Task<IReadOnlyList<EmpleadoRevisableDto>> Handle(
        GetEmpleadosRevisablesQuery request,
        CancellationToken cancellationToken)
    {
        var revisorId = usuario.Id;
        if (string.IsNullOrWhiteSpace(revisorId)) return [];

        return await repositorio.GetEmpleadosRevisablesAsync(revisorId, cancellationToken);
    }
}
