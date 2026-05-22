using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class RegistroHorasTests
{
    [Fact]
    public void Constructor_WhenHoraSalidaAMIsBeforeEntrada_ShouldThrow()
    {
        var act = () => CreateRegistro(
            horaEntradaAM: new TimeOnly(13, 0),
            horaSalidaAM: new TimeOnly(8, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*hora de salida*");
    }

    [Fact]
    public void Constructor_WhenNoTurnoProvided_ShouldThrow()
    {
        var act = () => new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            null, null, null, null,
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*al menos un turno*");
    }

    [Fact]
    public void Constructor_WhenRequiredFieldIsBlank_ShouldThrow()
    {
        var act = () => CreateRegistro(cliente: " ");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WhenValid_ShouldTrimAndAssignValues()
    {
        var registro = CreateRegistro(cliente: " KPG ");

        registro.Cliente.Should().Be("KPG");
        registro.TieneAM.Should().BeTrue();
        registro.TienePM.Should().BeFalse();
        registro.UserId.Should().Be("user-1");
    }

    [Fact]
    public void Constructor_WhenBothTurnos_ShouldSetBothBlocks()
    {
        var registro = new RegistroHoras(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(12, 0),
            new TimeOnly(13, 0), new TimeOnly(17, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        registro.TieneAM.Should().BeTrue();
        registro.TienePM.Should().BeTrue();
    }

    private static RegistroHoras CreateRegistro(
        string cliente = "KPG",
        TimeOnly? horaEntradaAM = null,
        TimeOnly? horaSalidaAM = null) =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            horaEntradaAM ?? new TimeOnly(8, 0),
            horaSalidaAM ?? new TimeOnly(13, 0),
            null,
            null,
            cliente,
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
