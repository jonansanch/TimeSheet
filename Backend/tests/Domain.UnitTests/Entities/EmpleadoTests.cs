using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class EmpleadoTests
{
    [Fact]
    public void Constructor_NombreValido_CreaEmpleadoActivo()
    {
        var empleado = new Empleado("Consultor");

        empleado.Nombre.Should().Be("Consultor");
        empleado.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NombreConEspacios_TrimeaNombre()
    {
        var empleado = new Empleado("  Analista  ");

        empleado.Nombre.Should().Be("Analista");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NombreVacio_LanzaDomainRuleException(string nombre)
    {
        var act = () => new Empleado(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_NombreMayorA200Caracteres_LanzaDomainRuleException()
    {
        var nombreLargo = new string('A', 201);
        var act = () => new Empleado(nombreLargo);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Desactivar_EmpleadoActivo_EstableceActivoFalse()
    {
        var empleado = new Empleado("Consultor");

        empleado.Desactivar();

        empleado.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_EmpleadoInactivo_EstableceActivoTrue()
    {
        var empleado = new Empleado("Consultor");
        empleado.Desactivar();

        empleado.Activar();

        empleado.Activo.Should().BeTrue();
    }

    [Fact]
    public void ActualizarNombre_NombreValido_ActualizaNombre()
    {
        var empleado = new Empleado("Consultor");

        empleado.ActualizarNombre("Analista");

        empleado.Nombre.Should().Be("Analista");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ActualizarNombre_NombreVacio_LanzaDomainRuleException(string nombre)
    {
        var empleado = new Empleado("Consultor");

        var act = () => empleado.ActualizarNombre(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void ActualizarNombre_NombreMayorA200Caracteres_LanzaDomainRuleException()
    {
        var empleado = new Empleado("Consultor");
        var nombreLargo = new string('A', 201);

        var act = () => empleado.ActualizarNombre(nombreLargo);

        act.Should().Throw<DomainRuleException>();
    }
}
