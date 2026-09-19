using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetVentanaRetroactividad;

/// <summary>
/// Devuelve la ventana <b>efectiva del usuario autenticado</b>, no la global: el formulario
/// y el calendario de registro deben bloquear exactamente las fechas que el backend rechaza.
/// La pantalla de parametros usa <see cref="IVentanaRetroactividadService.GetDiasGlobalAsync"/>.
/// </summary>
public class GetVentanaRetroactividadQueryHandler(
    IVentanaRetroactividadService ventana,
    IUser user)
    : IRequestHandler<GetVentanaRetroactividadQuery, int>
{
    public Task<int> Handle(GetVentanaRetroactividadQuery request, CancellationToken cancellationToken)
        => ventana.GetDiasAsync(user.Id, user.Roles, cancellationToken);
}
