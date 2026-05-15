using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class ModalidadTests
{
    [Fact]
    public void Constructor_WithNombreValido_CreatesModalidadActiva()
    {
        var modalidad = new Modalidad("Presencial");

        modalidad.Nombre.Should().Be("Presencial");
        modalidad.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_TrimsNombre()
    {
        var modalidad = new Modalidad("  Remoto  ");

        modalidad.Nombre.Should().Be("Remoto");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNombreVacio_ThrowsDomainRuleException(string nombre)
    {
        var act = () => new Modalidad(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WithNombreMayor100Chars_ThrowsDomainRuleException()
    {
        var nombre = new string('A', 101);

        var act = () => new Modalidad(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Desactivar_SetsActivoFalse()
    {
        var modalidad = new Modalidad("Presencial");

        modalidad.Desactivar();

        modalidad.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_SetsActivoTrue()
    {
        var modalidad = new Modalidad("Presencial");
        modalidad.Desactivar();

        modalidad.Activar();

        modalidad.Activo.Should().BeTrue();
    }

    [Fact]
    public void ActualizarNombre_WithNombreValido_UpdatesNombre()
    {
        var modalidad = new Modalidad("Presencial");

        modalidad.ActualizarNombre("Hibrido");

        modalidad.Nombre.Should().Be("Hibrido");
    }

    [Fact]
    public void ActualizarNombre_WithNombreVacio_ThrowsDomainRuleException()
    {
        var modalidad = new Modalidad("Presencial");

        var act = () => modalidad.ActualizarNombre("");

        act.Should().Throw<DomainRuleException>();
    }
}
