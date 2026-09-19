namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Avisa al empleado de lo que pasa con sus registros en la cadena de aprobacion.
///
/// <para>
/// <b>Nunca lanza.</b> Un fallo de correo no puede tumbar un rechazo ya decidido: el
/// estado del registro es lo importante, el aviso es un extra. Los fallos se registran
/// en el log.
/// </para>
/// </summary>
public interface INotificadorAprobacion
{
    Task NotificarRechazoAsync(
        string empleadoUserId,
        DateOnly fechaRegistro,
        string proyecto,
        string comentario,
        CancellationToken cancellationToken = default);
}
