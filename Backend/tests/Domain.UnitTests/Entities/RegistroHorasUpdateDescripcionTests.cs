using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class RegistroHorasUpdateDescripcionTests
{
    private static RegistroHoras CreateRegistro(string descripcion = "Descripción original") =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            null,
            null,
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            descripcion,
            "Bogota");

    [Fact]
    public void UpdateDescripcion_WhenDescripcionValida_ActualizaDescripcion()
    {
        var registro = CreateRegistro();
        registro.UpdateDescripcion("Nueva descripción");
        registro.Descripcion.Should().Be("Nueva descripción");
    }

    [Fact]
    public void UpdateDescripcion_TrimsDescripcion()
    {
        var registro = CreateRegistro();
        registro.UpdateDescripcion("  descripción con espacios  ");
        registro.Descripcion.Should().Be("descripción con espacios");
    }

    [Fact]
    public void UpdateDescripcion_WhenDescripcionVacia_LanzaDomainRuleException()
    {
        var registro = CreateRegistro();
        var act = () => registro.UpdateDescripcion("   ");
        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void UpdateDescripcion_WhenDescripcionExcede1000Chars_LanzaDomainRuleException()
    {
        var registro = CreateRegistro();
        var descripcionLarga = new string('x', 1001);
        var act = () => registro.UpdateDescripcion(descripcionLarga);
        act.Should().Throw<DomainRuleException>()
            .WithMessage("*1000*");
    }
}
