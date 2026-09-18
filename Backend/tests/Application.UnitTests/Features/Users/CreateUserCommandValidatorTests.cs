using KPG.Timesheet.Application.Features.Users.Commands.CreateUser;
using KPG.Timesheet.Domain.Constants;
using FluentAssertions;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRoleInvalid_ShouldFail()
    {
        var result = _validator.Validate(ValidCommand() with { Role = "Root" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Role));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenNombreCompletoIsMissing_ShouldFail(string? nombre)
    {
        var result = _validator.Validate(ValidCommand() with { NombreCompleto = nombre });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.NombreCompleto));
    }

    [Fact]
    public void Validate_WhenNombreCompletoExceeds200Chars_ShouldFail()
    {
        var result = _validator.Validate(ValidCommand() with { NombreCompleto = new string('x', 201) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.NombreCompleto));
    }

    private static CreateUserCommand ValidCommand() =>
        new("nuevo@kpg.com", "Empleado1234!", Roles.Empleado, "Nuevo Usuario");
}
