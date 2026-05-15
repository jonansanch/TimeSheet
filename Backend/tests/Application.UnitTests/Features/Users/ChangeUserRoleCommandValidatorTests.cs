using FluentAssertions;
using KPG.Timesheet.Application.Features.Users.Commands.ChangeUserRole;
using KPG.Timesheet.Domain.Constants;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Users;

public class ChangeUserRoleCommandValidatorTests
{
    private readonly ChangeUserRoleCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldPass()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand("user-id", Roles.Supervisor));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRoleEmpty_ShouldFail()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand("user-id", string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeUserRoleCommand.Role));
    }

    [Fact]
    public void Validate_WhenRoleInvalid_ShouldFail()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand("user-id", "Root"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeUserRoleCommand.Role));
    }

    [Fact]
    public void Validate_WhenUserIdEmpty_ShouldFail()
    {
        var result = _validator.Validate(new ChangeUserRoleCommand(string.Empty, Roles.Empleado));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangeUserRoleCommand.UserId));
    }
}
