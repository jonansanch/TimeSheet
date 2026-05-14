using FluentAssertions;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Enums;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.RegistroHoras;

public class CreateRegistroHorasCommandValidatorTests
{
    private readonly CreateRegistroHorasCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenRequiredFieldsAreEmpty_ShouldFail()
    {
        var command = ValidCommand() with
        {
            Cliente = string.Empty,
            Proyecto = string.Empty,
            Modalidad = string.Empty,
            Recurso = string.Empty,
            Descripcion = string.Empty,
            Lugar = string.Empty
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Cliente));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Proyecto));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Modalidad));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Recurso));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Descripcion));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Lugar));
    }

    [Fact]
    public async Task Validate_WhenHoraSalidaIsNotAfterEntrada_ShouldFail()
    {
        var command = ValidCommand() with
        {
            HoraEntrada = new TimeOnly(13, 0),
            HoraSalida = new TimeOnly(13, 0)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HoraSalida));
    }

    [Fact]
    public async Task Validate_WhenValid_ShouldPass()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(
            new DateOnly(2026, 5, 14),
            TurnoRegistro.AM,
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
