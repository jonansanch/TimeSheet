using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class LugarTrabajoTests
{
    [Fact]
    public void Constructor_WithNombreValido_CreatesLugarTrabajoActivo()
    {
        var lugar = new LugarTrabajo("Presencial Oficina");

        lugar.Nombre.Should().Be("Presencial Oficina");
        lugar.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_TrimsNombre()
    {
        var lugar = new LugarTrabajo("  Remoto  ");

        lugar.Nombre.Should().Be("Remoto");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNombreVacio_ThrowsDomainRuleException(string nombre)
    {
        var act = () => new LugarTrabajo(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WithNombreMayor200Chars_ThrowsDomainRuleException()
    {
        var nombre = new string('A', 201);

        var act = () => new LugarTrabajo(nombre);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Desactivar_SetsActivoFalse()
    {
        var lugar = new LugarTrabajo("Presencial Oficina");

        lugar.Desactivar();

        lugar.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_SetsActivoTrue()
    {
        var lugar = new LugarTrabajo("Presencial Oficina");
        lugar.Desactivar();

        lugar.Activar();

        lugar.Activo.Should().BeTrue();
    }

    [Fact]
    public void ActualizarNombre_WithNombreValido_UpdatesNombre()
    {
        var lugar = new LugarTrabajo("Presencial Oficina");

        lugar.ActualizarNombre("Presencial Cliente");

        lugar.Nombre.Should().Be("Presencial Cliente");
    }

    [Fact]
    public void ActualizarNombre_WithNombreVacio_ThrowsDomainRuleException()
    {
        var lugar = new LugarTrabajo("Presencial Oficina");

        var act = () => lugar.ActualizarNombre("");

        act.Should().Throw<DomainRuleException>();
    }
}
