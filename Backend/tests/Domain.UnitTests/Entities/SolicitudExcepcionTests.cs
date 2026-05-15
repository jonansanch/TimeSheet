using FluentAssertions;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;
using Xunit;

namespace KPG.Timesheet.Domain.UnitTests.Entities;

public class SolicitudExcepcionTests
{
    private static SolicitudExcepcion CreateSolicitud() =>
        new("user-1", new DateOnly(2026, 4, 1), "Justificación de prueba");

    [Fact]
    public void Aprobar_WhenPendiente_CambiaEstadoAAprobada()
    {
        var s = CreateSolicitud();
        s.Aprobar();
        s.Estado.Should().Be(EstadoSolicitud.Aprobada);
    }

    [Fact]
    public void Rechazar_WhenPendiente_CambiaEstadoARechazada()
    {
        var s = CreateSolicitud();
        s.Rechazar();
        s.Estado.Should().Be(EstadoSolicitud.Rechazada);
    }

    [Fact]
    public void Aprobar_WhenYaAprobada_ThrowsDomainRuleException()
    {
        var s = CreateSolicitud();
        s.Aprobar();
        var act = () => s.Aprobar();
        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Rechazar_WhenYaRechazada_ThrowsDomainRuleException()
    {
        var s = CreateSolicitud();
        s.Rechazar();
        var act = () => s.Rechazar();
        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WhenUserIdBlank_ThrowsDomainRuleException()
    {
        var act = () => new SolicitudExcepcion(" ", new DateOnly(2026, 4, 1), "Justificacion");
        act.Should().Throw<DomainRuleException>();
    }

    [Fact]
    public void Constructor_WhenJustificacionBlank_ThrowsDomainRuleException()
    {
        var act = () => new SolicitudExcepcion("user-1", new DateOnly(2026, 4, 1), " ");
        act.Should().Throw<DomainRuleException>();
    }
}
