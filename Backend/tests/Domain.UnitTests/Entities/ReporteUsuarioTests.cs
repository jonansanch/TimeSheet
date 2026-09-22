using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class ReporteUsuarioTests
{
    [Fact]
    public void Constructor_WhenValid_ShouldSetNuevoState()
    {
        var reporte = new ReporteUsuario("user-1", TipoReporte.Falla, "No carga el dashboard", "Al entrar da error 500.");

        reporte.Estado.Should().Be(EstadoReporte.Nuevo);
        reporte.UserId.Should().Be("user-1");
        reporte.Tipo.Should().Be(TipoReporte.Falla);
    }

    [Fact]
    public void Constructor_WhenTituloIsBlank_ShouldThrow()
    {
        var act = () => new ReporteUsuario("user-1", TipoReporte.Mejora, "  ", "Descripcion valida");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WhenDescripcionIsBlank_ShouldThrow()
    {
        var act = () => new ReporteUsuario("user-1", TipoReporte.Mejora, "Titulo valido", "  ");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void MarcarEnRevision_FromNuevo_ShouldSetState()
    {
        var reporte = CreateReporte();

        reporte.MarcarEnRevision("admin-1", "Lo estamos viendo");

        reporte.Estado.Should().Be(EstadoReporte.EnRevision);
        reporte.RespondidoPorUserId.Should().Be("admin-1");
        reporte.ComentarioRespuesta.Should().Be("Lo estamos viendo");
    }

    [Fact]
    public void Resolver_FromNuevo_ShouldSetState()
    {
        var reporte = CreateReporte();

        reporte.Resolver("admin-1", "Corregido en el ultimo deploy");

        reporte.Estado.Should().Be(EstadoReporte.Resuelto);
    }

    [Fact]
    public void Resolver_WhenAlreadyResuelto_ShouldThrow()
    {
        var reporte = CreateReporte();
        reporte.Resolver("admin-1", null);

        var act = () => reporte.Resolver("admin-1", null);

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Rechazar_SinComentario_ShouldThrow()
    {
        var reporte = CreateReporte();

        var act = () => reporte.Rechazar("admin-1", "  ");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Rechazar_ConComentario_ShouldSetState()
    {
        var reporte = CreateReporte();

        reporte.Rechazar("admin-1", "No es reproducible");

        reporte.Estado.Should().Be(EstadoReporte.Rechazado);
        reporte.ComentarioRespuesta.Should().Be("No es reproducible");
    }

    [Fact]
    public void Rechazar_WhenYaResuelto_ShouldThrow()
    {
        var reporte = CreateReporte();
        reporte.Resolver("admin-1", null);

        var act = () => reporte.Rechazar("admin-1", "motivo");

        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void MarcarEnRevision_WhenYaCerrado_ShouldThrow()
    {
        var reporte = CreateReporte();
        reporte.Rechazar("admin-1", "motivo");

        var act = () => reporte.MarcarEnRevision("admin-1", null);

        act.Should().Throw<DomainRuleException>();
    }

    private static ReporteUsuario CreateReporte() =>
        new("user-1", TipoReporte.Falla, "Titulo", "Descripcion del problema encontrado.");
}
