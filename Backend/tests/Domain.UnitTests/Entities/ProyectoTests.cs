using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class ProyectoTests
{
    [Fact]
    public void Constructor_WithDatosValidos_CreatesProyectoActivo()
    {
        var proyecto = new Proyecto(1, "Timesheet");

        proyecto.Nombre.Should().Be("Timesheet");
        proyecto.ClienteId.Should().Be(1);
        proyecto.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_TrimsNombre()
    {
        var proyecto = new Proyecto(1, "  Timesheet  ");

        proyecto.Nombre.Should().Be("Timesheet");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithClienteIdInvalido_ThrowsDomainRuleException(int clienteId)
    {
        var act = () => new Proyecto(clienteId, "Timesheet");

        act.Should().Throw<DomainRuleException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNombreVacio_ThrowsDomainRuleException(string nombre)
    {
        var act = () => new Proyecto(1, nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WithNombreMayor200Chars_ThrowsDomainRuleException()
    {
        var nombre = new string('A', 201);

        var act = () => new Proyecto(1, nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Desactivar_SetsActivoFalse()
    {
        var proyecto = new Proyecto(1, "Timesheet");

        proyecto.Desactivar();

        proyecto.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_SetsActivoTrue()
    {
        var proyecto = new Proyecto(1, "Timesheet");
        proyecto.Desactivar();

        proyecto.Activar();

        proyecto.Activo.Should().BeTrue();
    }

    [Fact]
    public void ActualizarNombre_WithNombreValido_UpdatesNombre()
    {
        var proyecto = new Proyecto(1, "Timesheet");

        proyecto.ActualizarNombre("Timesheet v2");

        proyecto.Nombre.Should().Be("Timesheet v2");
    }

    [Fact]
    public void ActualizarNombre_WithNombreVacio_ThrowsDomainRuleException()
    {
        var proyecto = new Proyecto(1, "Timesheet");

        var act = () => proyecto.ActualizarNombre("");

        act.Should().Throw<DomainRuleException>();
    }
}
