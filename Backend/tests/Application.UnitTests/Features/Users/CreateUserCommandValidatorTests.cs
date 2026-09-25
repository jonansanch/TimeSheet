using KPG.Timesheet.Application.Features.Users.Commands.CreateUser;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void CountryCatalog_ShouldContainExactlyIso3166Alpha2Codes()
    {
        CodigoPaisIso.TodosLosCodigos.Should().HaveCount(249);
        CodigoPaisIso.TodosLosCodigos.Should().Contain("AQ");
        CodigoPaisIso.TodosLosCodigos.Should().NotContain("ZZ");
    }

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

    [Theory]
    [InlineData("")]
    [InlineData("C")]
    [InlineData("ZZ")]
    public void Validate_WhenCodigoPaisIsInvalid_ShouldFail(string codigoPais)
    {
        var result = _validator.Validate(ValidCommand() with { CodigoPais = codigoPais });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.CodigoPais));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("CO")]
    [InlineData("co")]
    public void Validate_WhenCodigoPaisIsNullOrKnown_ShouldPass(string? codigoPais)
    {
        var result = _validator.Validate(ValidCommand() with { CodigoPais = codigoPais });

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUserCommand.CodigoPais));
    }

    private static CreateUserCommand ValidCommand() =>
        new("nuevo@kpg.com", "Empleado1234!", Roles.Empleado, "Nuevo Usuario");
}
