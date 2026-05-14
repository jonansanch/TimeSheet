using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class RegistroHorasTests
{
    [Fact]
    public void Constructor_WhenHoraSalidaIsBeforeEntrada_ShouldThrow()
    {
        var act = () => CreateRegistro(
            horaEntrada: new TimeOnly(13, 0),
            horaSalida: new TimeOnly(8, 0));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*hora de salida*");
    }

    [Fact]
    public void Constructor_WhenRequiredFieldIsBlank_ShouldThrow()
    {
        var act = () => CreateRegistro(cliente: " ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenValid_ShouldTrimAndAssignValues()
    {
        var registro = CreateRegistro(cliente: " KPG ");

        registro.Cliente.Should().Be("KPG");
        registro.Turno.Should().Be(TurnoRegistro.AM);
        registro.UserId.Should().Be("user-1");
    }

    private static RegistroHoras CreateRegistro(
        string cliente = "KPG",
        TimeOnly? horaEntrada = null,
        TimeOnly? horaSalida = null) =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            TurnoRegistro.AM,
            horaEntrada ?? new TimeOnly(8, 0),
            horaSalida ?? new TimeOnly(13, 0),
            cliente,
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
