namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Convierte lo que el empleado dicto en los campos del registro.
///
/// <para>
/// Vive detras del backend a proposito: la llamada al modelo necesita una API key que no
/// puede viajar al navegador. El frontend manda el texto y recibe los campos.
/// </para>
/// </summary>
public interface IInterpreteVoz
{
    /// <summary>
    /// False cuando no hay API key configurada. El frontend usa entonces su parser de
    /// reglas, que no necesita red ni credenciales.
    /// </summary>
    bool Disponible { get; }

    Task<InterpretacionVozDto> InterpretarAsync(
        string transcripcion,
        CatalogoVozDto catalogo,
        DateOnly hoy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Lo que el modelo puede elegir. Se le manda el catalogo real para que no invente
/// nombres de clientes ni proyectos que no existen.
/// </summary>
public record CatalogoVozDto(
    IReadOnlyList<string> Clientes,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ProyectosPorCliente,
    IReadOnlyList<string> Modalidades,
    IReadOnlyList<string> Recursos,
    IReadOnlyList<string> Lugares);

/// <summary>
/// Campos detectados. Todo es opcional: lo que el modelo no encuentra queda en null y el
/// formulario lo deja como estaba, en vez de pisarlo con un valor inventado.
/// </summary>
public record InterpretacionVozDto(
    DateOnly? Fecha = null,
    TimeOnly? HoraEntrada1 = null,
    TimeOnly? HoraSalida1 = null,
    TimeOnly? HoraEntrada2 = null,
    TimeOnly? HoraSalida2 = null,
    TimeOnly? HoraEntrada3 = null,
    TimeOnly? HoraSalida3 = null,
    string? Cliente = null,
    string? Proyecto = null,
    string? Modalidad = null,
    string? Recurso = null,
    string? Lugar = null,
    string? Descripcion = null)
{
    public static InterpretacionVozDto Vacia => new();
}
