using FluentAssertions;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
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
            Cliente     = string.Empty,
            Proyecto    = string.Empty,
            Modalidad   = string.Empty,
            Recurso     = string.Empty,
            Descripcion = string.Empty,
            Lugar       = string.Empty
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
    public async Task Validate_WhenNoHorarioProvided_ShouldFail()
    {
        var command = ValidCommand() with
        {
            HoraEntrada1 = null,
            HoraSalida1  = null,
            HoraEntrada2 = null,
            HoraSalida2  = null,
            HoraEntrada3 = null,
            HoraSalida3  = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenHoraSalida1IsNotAfterEntrada_ShouldFail()
    {
        var command = ValidCommand() with
        {
            HoraEntrada1 = new TimeOnly(13, 0),
            HoraSalida1  = new TimeOnly(13, 0)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HoraSalida1));
    }

    [Fact]
    public async Task Validate_WhenHoraSalida2IsNotAfterEntrada_ShouldFail()
    {
        var command = new CreateRegistroHorasCommand(
            new DateOnly(2026, 5, 14),
            null, null,
            new TimeOnly(14, 0), new TimeOnly(13, 0),
            null, null,
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HoraSalida2));
    }

    [Fact]
    public async Task Validate_WhenHoraSalida3IsNotAfterEntrada_ShouldFail()
    {
        var command = new CreateRegistroHorasCommand(
            new DateOnly(2026, 5, 14),
            null, null,
            null, null,
            new TimeOnly(20, 0), new TimeOnly(19, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HoraSalida3));
    }

    [Fact]
    public async Task Validate_WhenHorario3HasOnlyEntrada_ShouldFail()
    {
        var command = ValidCommand() with { HoraEntrada3 = new TimeOnly(19, 0) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.HoraSalida3));
    }

    [Fact]
    public async Task Validate_WhenValid_ShouldPass()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenThreeHorariosValid_ShouldPass()
    {
        var command = new CreateRegistroHorasCommand(
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),  new TimeOnly(12, 0),
            new TimeOnly(13, 0), new TimeOnly(17, 0),
            new TimeOnly(19, 0), new TimeOnly(21, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenOnlyHorario3Provided_ShouldPass()
    {
        var command = new CreateRegistroHorasCommand(
            new DateOnly(2026, 5, 14),
            null, null,
            null, null,
            new TimeOnly(19, 0), new TimeOnly(21, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            null,
            null,
            null,
            null,
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");
}
