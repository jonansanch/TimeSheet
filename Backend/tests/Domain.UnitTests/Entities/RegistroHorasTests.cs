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
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*al menos un horario*");
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
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

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
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

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
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        registro.TotalMinutos.Should().Be(555);
    }

    [Fact]
    public void SetBloque_WhenBlockIsEmpty_ShouldAssignIt()
    {
        var registro = CreateRegistro();

        registro.SetBloque(3, new TimeOnly(19, 0), new TimeOnly(21, 0));

        registro.TieneHorario3.Should().BeTrue();
        registro.HoraSalida3.Should().Be(new TimeOnly(21, 0));
    }

    [Fact]
    public void SetBloque_WhenBlockAlreadyRegistered_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.SetBloque(1, new TimeOnly(9, 0), new TimeOnly(10, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*ya fue registrado*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void SetBloque_WhenNumeroIsOutOfRange_ShouldThrow(int numero)
    {
        var registro = CreateRegistro();

        var act = () => registro.SetBloque(numero, new TimeOnly(19, 0), new TimeOnly(21, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*entre 1 y 3*");
    }

    [Fact]
    public void SetBloque_WhenSalidaIsNotAfterEntrada_ShouldThrow()
    {
        var registro = CreateRegistro();

        var act = () => registro.SetBloque(2, new TimeOnly(19, 0), new TimeOnly(19, 0));

        act.Should().Throw<DomainRuleException>()
            .WithMessage("*hora de salida*");
    }

    private static RegistroHoras CreateRegistro(
        string cliente = "KPG",
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
            cliente,
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
