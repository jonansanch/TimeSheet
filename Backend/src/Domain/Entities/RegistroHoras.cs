using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class RegistroHoras : BaseAuditableEntity
{
    /// <summary>Cantidad de bloques horarios que admite un registro diario.</summary>
    public const int TotalBloques = 3;

    private RegistroHoras() { }

    public RegistroHoras(
        string userId,
        DateOnly fechaRegistro,
        TimeOnly? horaEntrada1,
        TimeOnly? horaSalida1,
        TimeOnly? horaEntrada2,
        TimeOnly? horaSalida2,
        TimeOnly? horaEntrada3,
        TimeOnly? horaSalida3,
        int proyectoId,
        string clienteNombre,
        string proyectoNombre,
        string modalidad,
        string recurso,
        string descripcion,
        string lugar,
        bool esRetroactivo = false)
    {
        ThrowIfBlank(userId, nameof(userId));
        if (proyectoId <= 0)
            throw new DomainRuleException("El proyecto es requerido.");
        ThrowIfBlank(clienteNombre, nameof(clienteNombre));
        ThrowIfBlank(proyectoNombre, nameof(proyectoNombre));
        ThrowIfBlank(modalidad, nameof(modalidad));
        ThrowIfBlank(recurso, nameof(recurso));
        ThrowIfBlank(descripcion, nameof(descripcion));
        ThrowIfBlank(lugar, nameof(lugar));

        ValidateBloque(horaEntrada1, horaSalida1, 1);
        ValidateBloque(horaEntrada2, horaSalida2, 2);
        ValidateBloque(horaEntrada3, horaSalida3, 3);

        if (!horaEntrada1.HasValue && !horaEntrada2.HasValue && !horaEntrada3.HasValue)
            throw new DomainRuleException("Debe registrar al menos un horario.");

        // Misma regla que al agregar un horario despues: los tramos del dia no se cruzan.
        ValidarSinCruces(
            (horaEntrada1, horaSalida1, 1),
            (horaEntrada2, horaSalida2, 2),
            (horaEntrada3, horaSalida3, 3));

        UserId        = userId;
        FechaRegistro = fechaRegistro;
        HoraEntrada1  = horaEntrada1;
        HoraSalida1   = horaSalida1;
        HoraEntrada2  = horaEntrada2;
        HoraSalida2   = horaSalida2;
        HoraEntrada3  = horaEntrada3;
        HoraSalida3   = horaSalida3;
        ProyectoId     = proyectoId;
        ClienteNombre  = clienteNombre.Trim();
        ProyectoNombre = proyectoNombre.Trim();
        Modalidad      = modalidad.Trim();
        Recurso       = recurso.Trim();
        Descripcion   = descripcion.Trim();
        Lugar         = lugar.Trim();
        EsRetroactivo = esRetroactivo;
    }

    public string  UserId        { get; private set; } = string.Empty;
    public DateOnly FechaRegistro { get; private set; }

    public TimeOnly? HoraEntrada1 { get; private set; }
    public TimeOnly? HoraSalida1  { get; private set; }
    public TimeOnly? HoraEntrada2 { get; private set; }
    public TimeOnly? HoraSalida2  { get; private set; }
    public TimeOnly? HoraEntrada3 { get; private set; }
    public TimeOnly? HoraSalida3  { get; private set; }

    /// <summary>
    /// Proyecto al que se imputan las horas. El cliente se deduce por el proyecto
    /// (<see cref="Proyecto.ClienteId"/>), que es la unica fuente de esa relacion.
    /// Es la clave para agrupar: un proyecto renombrado no parte sus totales en dos.
    /// </summary>
    public int ProyectoId { get; private set; }

    /// <summary>
    /// Nombre del cliente <b>en el momento del registro</b>. Es una foto, no un espejo del
    /// catalogo: si manana se renombra el cliente, este registro conserva como se llamaba
    /// cuando se hizo el trabajo. Inmutable.
    /// </summary>
    public string ClienteNombre { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre del proyecto <b>en el momento del registro</b>. Ver <see cref="ClienteNombre"/>.
    /// </summary>
    public string ProyectoNombre { get; private set; } = string.Empty;

    public string Modalidad   { get; private set; } = string.Empty;
    public string Recurso     { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Lugar       { get; private set; } = string.Empty;
    public bool   EsRetroactivo { get; private set; }

    // ── Aprobacion ───────────────────────────────────────────────────────────

    public EstadoAprobacion Estado { get; private set; } = EstadoAprobacion.Pendiente;

    /// <summary>
    /// Estado que tenia antes de ser rechazado. Permite deshacer un rechazo equivocado
    /// devolviendo el registro al punto exacto de la cadena en el que estaba.
    /// </summary>
    public EstadoAprobacion? EstadoPrevioAlRechazo { get; private set; }

    /// <summary>Motivo del rechazo. Obligatorio al rechazar; se limpia al reenviar o revertir.</summary>
    public string? ComentarioRechazo { get; private set; }

    public bool EstaAprobado => Estado == EstadoAprobacion.Aprobado;
    public bool EstaRechazado => Estado == EstadoAprobacion.Rechazado;

    /// <summary>Nivel que corresponde revisar ahora (1-3), o null si no hay nada pendiente.</summary>
    public int? NivelPendiente => Estado switch
    {
        EstadoAprobacion.Pendiente      => 1,
        EstadoAprobacion.AprobadoNivel1 => 2,
        EstadoAprobacion.AprobadoNivel2 => 3,
        _                                => null
    };

    /// <summary>
    /// Aprueba el nivel indicado. Solo avanza si es exactamente el nivel pendiente:
    /// asi no se puede saltar un nivel ni aprobar dos veces el mismo.
    /// </summary>
    public void Aprobar(int nivel)
    {
        if (Estado == EstadoAprobacion.Rechazado)
            throw new DomainRuleException(
                "El registro esta rechazado: el empleado debe corregirlo y reenviarlo antes de aprobarlo.");

        if (Estado == EstadoAprobacion.Aprobado)
            throw new DomainRuleException("El registro ya esta aprobado en todos los niveles.");

        if (nivel != NivelPendiente)
            throw new DomainRuleException(
                $"Corresponde aprobar el nivel {NivelPendiente}, no el {nivel}.");

        Estado = nivel switch
        {
            1 => EstadoAprobacion.AprobadoNivel1,
            2 => EstadoAprobacion.AprobadoNivel2,
            _ => EstadoAprobacion.Aprobado
        };
    }

    /// <summary>Rechaza el registro con un motivo. Vuelve al empleado desde cualquier nivel.</summary>
    public void Rechazar(string comentario)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            throw new DomainRuleException("El rechazo requiere un comentario que explique el motivo.");
        if (comentario.Trim().Length > 1000)
            throw new DomainRuleException("El comentario de rechazo no puede superar 1000 caracteres.");

        if (Estado == EstadoAprobacion.Rechazado)
            throw new DomainRuleException("El registro ya esta rechazado.");

        EstadoPrevioAlRechazo = Estado;
        Estado                = EstadoAprobacion.Rechazado;
        ComentarioRechazo     = comentario.Trim();
    }

    /// <summary>
    /// Deshace la ultima aprobacion, retrocediendo un nivel. Para cuando un supervisor
    /// aprueba por error.
    /// </summary>
    public void RevertirAprobacion()
    {
        Estado = Estado switch
        {
            EstadoAprobacion.AprobadoNivel1 => EstadoAprobacion.Pendiente,
            EstadoAprobacion.AprobadoNivel2 => EstadoAprobacion.AprobadoNivel1,
            EstadoAprobacion.Aprobado       => EstadoAprobacion.AprobadoNivel2,
            EstadoAprobacion.Pendiente      => throw new DomainRuleException(
                "El registro no tiene ninguna aprobacion que revertir."),
            _                                => throw new DomainRuleException(
                "El registro esta rechazado: para deshacerlo usa revertir el rechazo.")
        };
    }

    /// <summary>
    /// Deshace un rechazo equivocado y devuelve el registro al punto de la cadena en el
    /// que estaba, sin obligar al empleado a reenviarlo.
    /// </summary>
    public void RevertirRechazo()
    {
        if (Estado != EstadoAprobacion.Rechazado)
            throw new DomainRuleException("El registro no esta rechazado.");

        Estado                = EstadoPrevioAlRechazo ?? EstadoAprobacion.Pendiente;
        EstadoPrevioAlRechazo = null;
        ComentarioRechazo     = null;
    }

    /// <summary>
    /// El empleado reenvia un registro rechazado tras corregirlo. La cadena recomienza
    /// desde el primer nivel: los aprobadores anteriores deben revisar la correccion.
    /// </summary>
    public void Reenviar()
    {
        if (Estado != EstadoAprobacion.Rechazado)
            throw new DomainRuleException("Solo se puede reenviar un registro rechazado.");

        Estado                = EstadoAprobacion.Pendiente;
        EstadoPrevioAlRechazo = null;
        ComentarioRechazo     = null;
    }

    // Computed helpers
    public bool TieneHorario1 => HoraEntrada1.HasValue;
    public bool TieneHorario2 => HoraEntrada2.HasValue;
    public bool TieneHorario3 => HoraEntrada3.HasValue;

    /// <summary>Suma en minutos de los bloques horarios completos del dia.</summary>
    public int TotalMinutos =>
        Minutos(HoraEntrada1, HoraSalida1) +
        Minutos(HoraEntrada2, HoraSalida2) +
        Minutos(HoraEntrada3, HoraSalida3);

    public void UpdateDescripcion(string nuevaDescripcion)
    {
        ThrowIfBlank(nuevaDescripcion, nameof(nuevaDescripcion));
        if (nuevaDescripcion.Length > 1000)
            throw new DomainRuleException("La descripcion no puede superar 1000 caracteres.");
        Descripcion = nuevaDescripcion.Trim();
    }

    /// <summary>
    /// Agrega un horario mas a la jornada del dia, en el primer bloque libre.
    ///
    /// <para>
    /// La unica condicion es que <b>no se cruce</b> con los ya registrados: si las horas no
    /// se solapan no hay conflicto, aunque el resto del registro sea identico. En que bloque
    /// termina es un detalle interno — quien registra piensa en "otro tramo del dia", no en
    /// un numero de casilla.
    /// </para>
    /// </summary>
    /// <returns>El numero de bloque donde quedo (1-3).</returns>
    public int AgregarHorario(TimeOnly horaEntrada, TimeOnly horaSalida)
    {
        if (horaSalida <= horaEntrada)
            throw new DomainRuleException("La hora de salida debe ser mayor que la hora de entrada.");
        if (horaEntrada.Minute % 15 != 0 || horaSalida.Minute % 15 != 0)
            throw new DomainRuleException(
                "Los minutos deben estar en cuartos de hora (00, 15, 30, 45).");

        for (var numero = 1; numero <= TotalBloques; numero++)
        {
            var entrada = GetHoraEntrada(numero);
            var salida  = GetHoraSalida(numero);

            if (entrada.HasValue && salida.HasValue &&
                SeCruzan(horaEntrada, horaSalida, entrada.Value, salida.Value))
                throw new DomainRuleException(
                    $"El horario {horaEntrada:HH\\:mm}-{horaSalida:HH\\:mm} se cruza con el " +
                    $"horario {numero} ya registrado ({entrada:HH\\:mm}-{salida:HH\\:mm}).");
        }

        var libre = PrimerBloqueLibre()
            ?? throw new DomainRuleException(
                $"El dia ya tiene los {TotalBloques} horarios registrados en este proyecto.");

        switch (libre)
        {
            case 1: HoraEntrada1 = horaEntrada; HoraSalida1 = horaSalida; break;
            case 2: HoraEntrada2 = horaEntrada; HoraSalida2 = horaSalida; break;
            case 3: HoraEntrada3 = horaEntrada; HoraSalida3 = horaSalida; break;
        }

        return libre;
    }

    /// <summary>Primer bloque sin horario, o null si los tres estan ocupados.</summary>
    private int? PrimerBloqueLibre()
    {
        for (var numero = 1; numero <= TotalBloques; numero++)
            if (!GetHoraEntrada(numero).HasValue)
                return numero;
        return null;
    }

    /// <summary>
    /// Dos tramos se cruzan si uno empieza antes de que el otro termine. Que se toquen
    /// (12:30 y 13:00) no es cruce: es una pausa entre jornadas.
    /// </summary>
    private static bool SeCruzan(TimeOnly entradaA, TimeOnly salidaA, TimeOnly entradaB, TimeOnly salidaB) =>
        entradaA < salidaB && entradaB < salidaA;

    public TimeOnly? GetHoraEntrada(int numero) => numero switch
    {
        1 => HoraEntrada1,
        2 => HoraEntrada2,
        3 => HoraEntrada3,
        _ => throw new DomainRuleException($"El numero de horario debe estar entre 1 y {TotalBloques}.")
    };

    public TimeOnly? GetHoraSalida(int numero) => numero switch
    {
        1 => HoraSalida1,
        2 => HoraSalida2,
        3 => HoraSalida3,
        _ => throw new DomainRuleException($"El numero de horario debe estar entre 1 y {TotalBloques}.")
    };

    /// <summary>
    /// El proyecto NO se actualiza aqui: identifica el registro junto con usuario y fecha,
    /// y cambiarlo convertiria este registro en otro distinto.
    /// </summary>
    public void UpdateMetadata(string modalidad, string recurso, string lugar)
    {
        ThrowIfBlank(modalidad, nameof(modalidad));
        ThrowIfBlank(recurso,   nameof(recurso));
        ThrowIfBlank(lugar,     nameof(lugar));
        Modalidad = modalidad.Trim();
        Recurso   = recurso.Trim();
        Lugar     = lugar.Trim();
    }

    private static int Minutos(TimeOnly? entrada, TimeOnly? salida) =>
        entrada.HasValue && salida.HasValue
            ? (int)(salida.Value - entrada.Value).TotalMinutes
            : 0;

    private static void ValidateBloque(TimeOnly? entrada, TimeOnly? salida, int numero)
    {
        if (entrada.HasValue != salida.HasValue)
            throw new DomainRuleException(
                $"El horario {numero} requiere tanto hora de entrada como hora de salida.");
        if (entrada.HasValue && salida!.Value <= entrada.Value)
            throw new DomainRuleException(
                $"La hora de salida del horario {numero} debe ser mayor que la hora de entrada.");
        if (entrada.HasValue)
        {
            ThrowIfNoEsCuartoDeHora(entrada.Value, numero, "entrada");
            ThrowIfNoEsCuartoDeHora(salida!.Value, numero, "salida");
        }
    }

    /// <summary>Los minutos solo pueden ser 0, 15, 30 o 45: igual que exige el formulario.</summary>
    private static void ThrowIfNoEsCuartoDeHora(TimeOnly hora, int numero, string tipo)
    {
        if (hora.Minute % 15 != 0)
            throw new DomainRuleException(
                $"La hora de {tipo} del horario {numero} debe estar en cuartos de hora (00, 15, 30, 45).");
    }

    private static void ValidarSinCruces(
        params (TimeOnly? Entrada, TimeOnly? Salida, int Numero)[] bloques)
    {
        var completos = bloques.Where(b => b.Entrada.HasValue && b.Salida.HasValue).ToArray();

        for (var i = 0; i < completos.Length; i++)
            for (var j = i + 1; j < completos.Length; j++)
                if (SeCruzan(completos[i].Entrada!.Value, completos[i].Salida!.Value,
                             completos[j].Entrada!.Value, completos[j].Salida!.Value))
                    throw new DomainRuleException(
                        $"Los horarios {completos[i].Numero} y {completos[j].Numero} se cruzan entre si.");
    }

    private static void ThrowIfBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException($"'{parameterName}' es requerido.");
    }
}
