using FluentAssertions;
using KPG.Timesheet.Application.Features.Auth.Commands.Login;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Validate_EmptyEmail_ShouldFail()
    {
        var command = new LoginCommand(string.Empty, "Password1!");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_ShouldFail()
    {
        var command = new LoginCommand("not-an-email", "Password1!");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public async Task Validate_EmptyPassword_ShouldFail()
    {
        var command = new LoginCommand("user@kpg.com", string.Empty);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public async Task Validate_ValidCredentials_ShouldPass()
    {
        var command = new LoginCommand("user@kpg.com", "Password1!");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }
}
