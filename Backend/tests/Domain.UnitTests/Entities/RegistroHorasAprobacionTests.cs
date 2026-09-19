using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

/// <summary>
/// Cadena de tres niveles: supervisor del puesto → jefe directo → responsable del proyecto.
/// Regla acordada con el cliente: <b>cualquier rechazo vuelve al empleado</b> y, al reenviar,
/// la cadena recomienza desde el primer nivel.
/// </summary>
public class RegistroHorasAprobacionTests
{
    [Fact]
    public void Nuevo_ShouldStartPendienteDelPrimerNivel()
    {
        var registro = CreateRegistro();

        registro.Estado.Should().Be(EstadoAprobacion.Pendiente);
        registro.NivelPendiente.Should().Be(1);
        registro.ComentarioRechazo.Should().BeNull();
    }

    [Fact]
    public void Aprobar_LosTresNiveles_ShouldTerminarAprobado()
    {
        var registro = CreateRegistro();

        registro.Aprobar(1);
        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel1);
        registro.NivelPendiente.Should().Be(2);

        registro.Aprobar(2);
        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel2);
        registro.NivelPendiente.Should().Be(3);

        registro.Aprobar(3);
        registro.Estado.Should().Be(EstadoAprobacion.Aprobado);
        registro.NivelPendiente.Should().BeNull();
        registro.EstaAprobado.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Aprobar_SaltandoUnNivel_ShouldThrow(int nivel)
    {
        // Nadie puede adelantarse: el nivel 2 no aprueba antes que el 1.
        var registro = CreateRegistro();

        var act = () => registro.Aprobar(nivel);

        act.Should().Throw<DomainRuleException>().WithMessage("*nivel 1*");
    }

    [Fact]
    public void Aprobar_ElMismoNivelDosVeces_ShouldThrow()
    {
        var registro = CreateRegistro();
        registro.Aprobar(1);

        var act = () => registro.Aprobar(1);

        act.Should().Throw<DomainRuleException>().WithMessage("*nivel 2*");
    }

    [Fact]
    public void Aprobar_CuandoYaEstaAprobado_ShouldThrow()
    {
        var registro = Aprobado();

        var act = () => registro.Aprobar(3);

        act.Should().Throw<DomainRuleException>().WithMessage("*ya esta aprobado*");
    }

    [Fact]
    public void Aprobar_CuandoEstaRechazado_ShouldThrow()
    {
        var registro = CreateRegistro();
        registro.Rechazar("Faltan horas del jueves.");

        var act = () => registro.Aprobar(1);

        act.Should().Throw<DomainRuleException>().WithMessage("*rechazado*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Rechazar_DesdeCualquierNivel_ShouldVolverAlEmpleado(int nivelesAprobados)
    {
        // La decision del cliente: el rechazo siempre llega al empleado, no al aprobador previo.
        var registro = CreateRegistro();
        for (var n = 1; n <= nivelesAprobados; n++) registro.Aprobar(n);

        registro.Rechazar("Las horas del martes no corresponden a este proyecto.");

        registro.Estado.Should().Be(EstadoAprobacion.Rechazado);
        registro.EstaRechazado.Should().BeTrue();
        registro.ComentarioRechazo.Should().Be("Las horas del martes no corresponden a este proyecto.");
        registro.NivelPendiente.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rechazar_SinComentario_ShouldThrow(string? comentario)
    {
        var registro = CreateRegistro();

        var act = () => registro.Rechazar(comentario!);

        act.Should().Throw<DomainRuleException>().WithMessage("*comentario*");
    }

    [Fact]
    public void Rechazar_ConComentarioMuyLargo_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.Rechazar(new string('x', 1001));

        act.Should().Throw<DomainRuleException>().WithMessage("*1000*");
    }

    [Fact]
    public void Rechazar_DosVeces_ShouldThrow()
    {
        var registro = CreateRegistro();
        registro.Rechazar("Motivo.");

        var act = () => registro.Rechazar("Otro motivo.");

        act.Should().Throw<DomainRuleException>().WithMessage("*ya esta rechazado*");
    }

    [Fact]
    public void Reenviar_ShouldRecomenzarDesdeElPrimerNivel()
    {
        // Aunque lo rechace el tercer nivel, la correccion debe pasar de nuevo por todos.
        var registro = CreateRegistro();
        registro.Aprobar(1);
        registro.Aprobar(2);
        registro.Rechazar("Corregir la descripcion.");

        registro.Reenviar();

        registro.Estado.Should().Be(EstadoAprobacion.Pendiente);
        registro.NivelPendiente.Should().Be(1);
        registro.ComentarioRechazo.Should().BeNull();
    }

    [Fact]
    public void Reenviar_CuandoNoEstaRechazado_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.Reenviar();

        act.Should().Throw<DomainRuleException>().WithMessage("*rechazado*");
    }

    [Fact]
    public void RevertirAprobacion_ShouldRetrocederUnSoloNivel()
    {
        var registro = CreateRegistro();
        registro.Aprobar(1);
        registro.Aprobar(2);

        registro.RevertirAprobacion();

        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel1);
        registro.NivelPendiente.Should().Be(2);
    }

    [Fact]
    public void RevertirAprobacion_DesdeAprobado_ShouldVolverAlTercerNivel()
    {
        var registro = Aprobado();

        registro.RevertirAprobacion();

        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel2);
        registro.NivelPendiente.Should().Be(3);
    }

    [Fact]
    public void RevertirAprobacion_SinAprobacionesPrevias_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.RevertirAprobacion();

        act.Should().Throw<DomainRuleException>().WithMessage("*ninguna aprobacion*");
    }

    [Fact]
    public void RevertirAprobacion_CuandoEstaRechazado_ShouldThrowIndicandoElMetodoCorrecto()
    {
        var registro = CreateRegistro();
        registro.Rechazar("Motivo.");

        var act = () => registro.RevertirAprobacion();

        act.Should().Throw<DomainRuleException>().WithMessage("*revertir el rechazo*");
    }

    [Fact]
    public void RevertirRechazo_ShouldDevolverAlPuntoExactoDeLaCadena()
    {
        // Deshacer un rechazo equivocado no debe obligar al empleado a reenviar ni
        // hacer que los niveles ya aprobados revisen de nuevo.
        var registro = CreateRegistro();
        registro.Aprobar(1);
        registro.Aprobar(2);
        registro.Rechazar("Me equivoque de persona.");

        registro.RevertirRechazo();

        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel2);
        registro.NivelPendiente.Should().Be(3);
        registro.ComentarioRechazo.Should().BeNull();
    }

    [Fact]
    public void RevertirRechazo_DesdePendiente_ShouldVolverAPendiente()
    {
        var registro = CreateRegistro();
        registro.Rechazar("Motivo.");

        registro.RevertirRechazo();

        registro.Estado.Should().Be(EstadoAprobacion.Pendiente);
    }

    [Fact]
    public void RevertirRechazo_CuandoNoEstaRechazado_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.RevertirRechazo();

        act.Should().Throw<DomainRuleException>().WithMessage("*no esta rechazado*");
    }

    [Fact]
    public void RechazarYRevertirDosVeces_ShouldSeguirDevolviendoAlPuntoCorrecto()
    {
        // El estado previo debe limpiarse al revertir; si no, un segundo rechazo
        // restauraria el punto equivocado.
        var registro = CreateRegistro();
        registro.Aprobar(1);
        registro.Rechazar("Primer motivo.");
        registro.RevertirRechazo();

        registro.Aprobar(2);
        registro.Rechazar("Segundo motivo.");
        registro.RevertirRechazo();

        registro.Estado.Should().Be(EstadoAprobacion.AprobadoNivel2);
    }

    private static RegistroHoras Aprobado()
    {
        var registro = CreateRegistro();
        registro.Aprobar(1);
        registro.Aprobar(2);
        registro.Aprobar(3);
        return registro;
    }

    private static RegistroHoras CreateRegistro() =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            1,
            "Cliente 1",
            "Proyecto 1",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
