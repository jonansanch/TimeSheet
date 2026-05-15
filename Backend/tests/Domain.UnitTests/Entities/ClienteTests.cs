using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class ClienteTests
{
    [Fact]
    public void Constructor_WithNombreValido_CreatesClienteActivo()
    {
        var cliente = new Cliente("KPG");

        cliente.Nombre.Should().Be("KPG");
        cliente.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_TrimsNombre()
    {
        var cliente = new Cliente("  KPG  ");

        cliente.Nombre.Should().Be("KPG");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNombreVacio_ThrowsDomainRuleException(string nombre)
    {
        var act = () => new Cliente(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WithNombreMayor200Chars_ThrowsDomainRuleException()
    {
        var nombre = new string('A', 201);

        var act = () => new Cliente(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Desactivar_SetsActivoFalse()
    {
        var cliente = new Cliente("KPG");

        cliente.Desactivar();

        cliente.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_SetsActivoTrue()
    {
        var cliente = new Cliente("KPG");
        cliente.Desactivar();

        cliente.Activar();

        cliente.Activo.Should().BeTrue();
    }

    [Fact]
    public void ActualizarNombre_WithNombreValido_UpdatesNombre()
    {
        var cliente = new Cliente("KPG");

        cliente.ActualizarNombre("KPG Nuevo");

        cliente.Nombre.Should().Be("KPG Nuevo");
    }

    [Fact]
    public void ActualizarNombre_WithNombreVacio_ThrowsDomainRuleException()
    {
        var cliente = new Cliente("KPG");

        var act = () => cliente.ActualizarNombre("");

        act.Should().Throw<DomainRuleException>();
    }
}
