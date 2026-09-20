using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IVozRepository
{
    /// <summary>
    /// False cuando el ambiente no tiene configurada la IA. El formulario usa entonces su
    /// parser de reglas, que corre en el navegador y no necesita red ni credenciales.
    /// </summary>
    Task<bool> EstaDisponibleAsync(CancellationToken cancellationToken = default);

    /// <summary>Null si no se pudo interpretar; quien llama cae a su alternativa local.</summary>
    Task<InterpretacionVozResponse?> InterpretarAsync(
        string transcripcion, CancellationToken cancellationToken = default);
}
