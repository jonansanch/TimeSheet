using KPG.Timesheet.WebUI.Infrastructure.Repositories;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Shared.Services;

public class CatalogosCacheService
{
    private readonly IModalidadRepository _modalidadRepo;
    private readonly ILugarTrabajoRepository _lugarTrabajoRepo;
    private readonly IEmpleadoRepository _empleadoRepo;

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private List<ModalidadResponse>? _modalidades;
    private DateTime _modalidadesAt = DateTime.MinValue;

    private List<LugarTrabajoResponse>? _lugaresTrabajo;
    private DateTime _lugaresTrabajoAt = DateTime.MinValue;

    private List<EmpleadoResponse>? _empleados;
    private DateTime _empleadosAt = DateTime.MinValue;

    public CatalogosCacheService(
        IModalidadRepository modalidadRepo,
        ILugarTrabajoRepository lugarTrabajoRepo,
        IEmpleadoRepository empleadoRepo)
    {
        _modalidadRepo = modalidadRepo;
        _lugarTrabajoRepo = lugarTrabajoRepo;
        _empleadoRepo = empleadoRepo;
    }

    public async Task<List<ModalidadResponse>> GetModalidadesActivasAsync(CancellationToken ct = default)
    {
        if (_modalidades is not null && DateTime.UtcNow - _modalidadesAt < Ttl)
            return _modalidades;

        _modalidades = await _modalidadRepo.GetActivasAsync(ct);
        _modalidadesAt = DateTime.UtcNow;
        return _modalidades;
    }

    public async Task<List<LugarTrabajoResponse>> GetLugaresTrabajoActivosAsync(CancellationToken ct = default)
    {
        if (_lugaresTrabajo is not null && DateTime.UtcNow - _lugaresTrabajoAt < Ttl)
            return _lugaresTrabajo;

        _lugaresTrabajo = await _lugarTrabajoRepo.GetActivosAsync(ct);
        _lugaresTrabajoAt = DateTime.UtcNow;
        return _lugaresTrabajo;
    }

    public async Task<List<EmpleadoResponse>> GetEmpleadosActivosAsync(CancellationToken ct = default)
    {
        if (_empleados is not null && DateTime.UtcNow - _empleadosAt < Ttl)
            return _empleados;

        _empleados = await _empleadoRepo.GetActivosAsync(ct);
        _empleadosAt = DateTime.UtcNow;
        return _empleados;
    }

    public void InvalidarModalidades() => _modalidades = null;
    public void InvalidarLugaresTrabajo() => _lugaresTrabajo = null;
    public void InvalidarEmpleados() => _empleados = null;
}
