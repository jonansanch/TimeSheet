using FluentAssertions;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.RechazarSolicitudExcepcion;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.SolicitudesExcepcion;

public class AprobarRechazarSolicitudExcepcionTests
{
    [Fact]
    public async Task Aprobar_WhenPendiente_CambiaEstadoAprobadaEnDb()
    {
        await using var context = CreateContext();
        var solicitud = new SolicitudExcepcion("user-1", new DateOnly(2026, 4, 1), "Justificación");
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AprobarSolicitudExcepcionCommandHandler(context);
        await handler.Handle(new AprobarSolicitudExcepcionCommand(solicitud.Id), CancellationToken.None);

        var updated = await context.SolicitudesExcepcion.FindAsync(solicitud.Id);
        updated!.Estado.Should().Be(EstadoSolicitud.Aprobada);
    }

    [Fact]
    public async Task Rechazar_WhenPendiente_CambiaEstadoRechazadaEnDb()
    {
        await using var context = CreateContext();
        var solicitud = new SolicitudExcepcion("user-1", new DateOnly(2026, 4, 1), "Justificación");
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RechazarSolicitudExcepcionCommandHandler(context);
        await handler.Handle(new RechazarSolicitudExcepcionCommand(solicitud.Id), CancellationToken.None);

        var updated = await context.SolicitudesExcepcion.FindAsync(solicitud.Id);
        updated!.Estado.Should().Be(EstadoSolicitud.Rechazada);
    }

    [Fact]
    public async Task Aprobar_WhenNoExiste_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new AprobarSolicitudExcepcionCommandHandler(context);

        var act = () => handler.Handle(new AprobarSolicitudExcepcionCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Aprobar_WhenYaAprobada_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var solicitud = new SolicitudExcepcion("user-1", new DateOnly(2026, 4, 1), "Justificación");
        solicitud.Aprobar();
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new AprobarSolicitudExcepcionCommandHandler(context);
        var act = () => handler.Handle(new AprobarSolicitudExcepcionCommand(solicitud.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainRuleException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
