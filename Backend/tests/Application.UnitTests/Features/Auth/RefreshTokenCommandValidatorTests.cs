using FluentAssertions;
using KPG.Timesheet.Application.Features.Auth.Commands.Refresh;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Auth;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public async Task Validate_EmptyToken_ShouldFail()
    {
        var command = new RefreshTokenCommand(string.Empty);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.RefreshToken));
    }

    [Fact]
    public async Task Validate_ValidToken_ShouldPass()
    {
        var command = new RefreshTokenCommand("valid-refresh-token-value");
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }
}
