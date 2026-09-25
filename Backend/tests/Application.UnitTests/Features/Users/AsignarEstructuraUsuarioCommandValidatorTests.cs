using FluentAssertions;
using KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Users;

public class AsignarEstructuraUsuarioCommandValidatorTests
{
    private readonly AsignarEstructuraUsuarioCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCountryUpdateIsOmitted_IgnoresCountryValue()
    {
        var command = new AsignarEstructuraUsuarioCommand("user-id", null, null, "ZZ");
        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AsignarEstructuraUsuarioCommand.CodigoPais));
    }

    [Theory]
    [InlineData("CO")]
    [InlineData("aq")]
    [InlineData(null)]
    public void Validate_WhenCountryUpdateIsExplicit_AcceptsIsoOrClear(string? codigo)
    {
        var command = new AsignarEstructuraUsuarioCommand(
            "user-id", null, null, codigo, ActualizarCodigoPais: true);
        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AsignarEstructuraUsuarioCommand.CodigoPais));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ZZ")]
    [InlineData("AAA")]
    public void Validate_WhenCountryUpdateIsExplicit_RejectsInvalidCode(string codigo)
    {
        var command = new AsignarEstructuraUsuarioCommand(
            "user-id", null, null, codigo, ActualizarCodigoPais: true);
        var result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == nameof(AsignarEstructuraUsuarioCommand.CodigoPais));
    }
}
