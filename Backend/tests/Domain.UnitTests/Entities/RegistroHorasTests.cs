using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class RegistroHorasTests
{
    [Fact]
    public void Constructor_WhenHoraSalida1IsBeforeEntrada_ShouldThrow()
    {
        var act = () => CreateRegistro(
            horaEntrada1: new TimeOnly(13, 0),
            horaSalida1: new TimeOnly(8, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*hora de salida*");
    }

    [Fact]
    public void Constructor_WhenNoHorarioProvided_ShouldThrow()
    {
        var act = () => new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            null, null, null, null, null, null,
            ProyectoIdValido, "Cliente 1", "Proyecto 1", "Remoto", "Consultor", "Desarrollo", "Bogota");

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*al menos un horario*");
    }

    [Fact]
    public void Constructor_WhenRequiredFieldIsBlank_ShouldThrow()
    {
        var act = () => CreateRegistro(proyectoId: 0);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WhenValid_ShouldTrimAndAssignValues()
    {
        var registro = CreateRegistro();

        registro.ProyectoId.Should().Be(ProyectoIdValido);
        registro.TieneHorario1.Should().BeTrue();
        registro.TieneHorario2.Should().BeFalse();
        registro.TieneHorario3.Should().BeFalse();
        registro.UserId.Should().Be("user-1");
    }

    [Fact]
    public void Constructor_WhenThreeHorarios_ShouldSetAllBlocks()
    {
        var registro = new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),  new TimeOnly(12, 0),
            new TimeOnly(13, 0), new TimeOnly(17, 0),
            new TimeOnly(19, 0), new TimeOnly(21, 0),
            ProyectoIdValido, "Cliente 1", "Proyecto 1", "Remoto", "Consultor", "Desarrollo", "Bogota");

        registro.TieneHorario1.Should().BeTrue();
        registro.TieneHorario2.Should().BeTrue();
        registro.TieneHorario3.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WhenOnlyHorario3Provided_ShouldBeValid()
    {
        var registro = new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            null, null,
            null, null,
            new TimeOnly(19, 0), new TimeOnly(21, 0),
            ProyectoIdValido, "Cliente 1", "Proyecto 1", "Remoto", "Consultor", "Desarrollo", "Bogota");

        registro.TieneHorario1.Should().BeFalse();
        registro.TieneHorario3.Should().BeTrue();
        registro.TotalMinutos.Should().Be(120);
    }

    [Fact]
    public void TotalMinutos_ShouldSumAllCompleteBlocks()
    {
        var registro = new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),  new TimeOnly(12, 0),   // 240
            new TimeOnly(13, 0), new TimeOnly(17, 30),  // 270
            new TimeOnly(19, 0), new TimeOnly(19, 45),  //  45
            ProyectoIdValido, "Cliente 1", "Proyecto 1", "Remoto", "Consultor", "Desarrollo", "Bogota");

        registro.TotalMinutos.Should().Be(555);
    }

    // ── Agregar horarios a la jornada ────────────────────────────────────────
    // La regla es el cruce, no la casilla: si las horas no se solapan, el horario entra.

    [Fact]
    public void AgregarHorario_WhenNoOverlap_ShouldUseFirstFreeBlock()
    {
        var registro = CreateRegistro();   // 08:00-13:00 en el bloque 1

        var bloque = registro.AgregarHorario(new TimeOnly(13, 0), new TimeOnly(18, 0));

        bloque.Should().Be(2);
        registro.HoraEntrada2.Should().Be(new TimeOnly(13, 0));
        registro.TotalMinutos.Should().Be(300 + 300);
    }

    [Fact]
    public void AgregarHorario_WhenBlocksTouch_ShouldAccept()
    {
        // 13:00 empieza justo cuando termina el anterior: es una pausa, no un cruce.
        var registro = CreateRegistro();

        var act = () => registro.AgregarHorario(new TimeOnly(13, 0), new TimeOnly(14, 0));

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(9, 0, 10, 0)]    // contenido dentro del existente
    [InlineData(12, 0, 15, 0)]   // empieza antes de que termine
    [InlineData(7, 0, 9, 0)]     // termina despues de que empieza
    [InlineData(8, 0, 13, 0)]    // identico
    public void AgregarHorario_WhenOverlaps_ShouldThrow(int he, int me, int hs, int ms)
    {
        var registro = CreateRegistro();   // 08:00-13:00

        var act = () => registro.AgregarHorario(new TimeOnly(he, me), new TimeOnly(hs, ms));

        act.Should().Throw<DomainRuleException>().WithMessage("*se cruza*");
    }

    [Fact]
    public void AgregarHorario_WhenAllBlocksTaken_ShouldThrow()
    {
        var registro = CreateRegistro();
        registro.AgregarHorario(new TimeOnly(13, 0), new TimeOnly(14, 0));
        registro.AgregarHorario(new TimeOnly(15, 0), new TimeOnly(16, 0));

        var act = () => registro.AgregarHorario(new TimeOnly(17, 0), new TimeOnly(18, 0));

        act.Should().Throw<DomainRuleException>().WithMessage("*ya tiene los 3 horarios*");
    }

    [Fact]
    public void AgregarHorario_WhenSalidaIsNotAfterEntrada_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.AgregarHorario(new TimeOnly(19, 0), new TimeOnly(19, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*hora de salida*");
    }

    [Fact]
    public void Constructor_WhenBlocksOverlapEachOther_ShouldThrow()
    {
        var act = () => new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),  new TimeOnly(13, 0),
            new TimeOnly(12, 0), new TimeOnly(17, 0),   // se cruza con el primero
            null, null,
            ProyectoIdValido, "Cliente 1", "Proyecto 1", "Remoto", "Consultor", "Desarrollo", "Bogota");

        act.Should().Throw<DomainRuleException>().WithMessage("*se cruzan*");
    }

    /// <summary>El dominio solo exige un id positivo; la existencia la valida la capa de aplicacion.</summary>
    private const int ProyectoIdValido = 1;

    private static RegistroHoras CreateRegistro(
        int proyectoId = ProyectoIdValido,
        TimeOnly? horaEntrada1 = null,
        TimeOnly? horaSalida1 = null) =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            horaEntrada1 ?? new TimeOnly(8, 0),
            horaSalida1 ?? new TimeOnly(13, 0),
            null,
            null,
            null,
            null,
            proyectoId,
            "Cliente 1",
            "Proyecto 1",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
