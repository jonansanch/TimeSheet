using FluentAssertions;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.SolicitudesExcepcion;

public class CreateSolicitudExcepcionCommandValidatorTests
{
    private readonly CreateSolicitudExcepcionCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenJustificacionIsEmpty_ShouldFail()
    {
        var command = ValidCommand() with { Justificacion = string.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Justificacion));
    }

    [Fact]
    public async Task Validate_WhenJustificacionTooLong_ShouldFail()
    {
        var command = ValidCommand() with { Justificacion = new string('a', 1001) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Justificacion));
    }

    [Fact]
    public async Task Validate_WhenValid_ShouldPass()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    private static CreateSolicitudExcepcionCommand ValidCommand() =>
        new(new DateOnly(2026, 4, 1), "Viaje de negocios imprevisto");
}
