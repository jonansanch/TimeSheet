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
        var result = _validator.Validate(new CreateUserCommand("nuevo@kpg.com", "Empleado1234!", Roles.Empleado));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRoleInvalid_ShouldFail()
    {
        var result = _validator.Validate(new CreateUserCommand("nuevo@kpg.com", "Empleado1234!", "Root"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Role));
    }
}
