namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IBitacoraService
{
    Task RegistrarAsync(
        string tipoEvento,
        string actorId,
        string? actorEmail,
        string entidadAfectada,
        string? entidadId,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
