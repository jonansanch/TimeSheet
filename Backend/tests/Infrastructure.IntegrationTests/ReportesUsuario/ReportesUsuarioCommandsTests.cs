using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.ReportesUsuario;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.ReportesUsuario;

public class ReportesUsuarioCommandsTests
{
    [Fact]
    public async Task CreateReporteUsuario_ShouldPersistAsNuevo()
    {
        await using var context = CreateContext();
        var handler = new CreateReporteUsuarioCommandHandler(context, new TestUser("user-1"), new NullBitacora());

        var result = await handler.Handle(
            new CreateReporteUsuarioCommand(TipoReporte.Falla, "No guarda", "Al presionar guardar no pasa nada."),
            CancellationToken.None);

        result.Estado.Should().Be(EstadoReporte.Nuevo);
        (await context.ReportesUsuario.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetMisReportes_ShouldReturnOnlyOwnReports()
    {
        await using var context = CreateContext();
        await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-1"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Falla, "A", "Descripcion A"), CancellationToken.None);
        await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-2"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Mejora, "B", "Descripcion B"), CancellationToken.None);

        var handler = new GetMisReportesQueryHandler(context, new TestUser("user-1"));
        var result = await handler.Handle(new GetMisReportesQuery(), CancellationToken.None);

        result.Should().ContainSingle(r => r.Titulo == "A");
    }

    [Fact]
    public async Task GetTodosReportes_ShouldReturnEveryonesReports()
    {
        await using var context = CreateContext();
        await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-1"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Falla, "A", "Descripcion A"), CancellationToken.None);
        await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-2"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Mejora, "B", "Descripcion B"), CancellationToken.None);

        var handler = new GetTodosReportesQueryHandler(context);
        var result = await handler.Handle(new GetTodosReportesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CambiarEstado_AResuelto_ShouldSetStateAndActor()
    {
        await using var context = CreateContext();
        var creado = await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-1"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Falla, "A", "Descripcion A"), CancellationToken.None);

        var handler = new CambiarEstadoReporteCommandHandler(context, new TestUser("admin-1"), new NullBitacora());
        var result = await handler.Handle(
            new CambiarEstadoReporteCommand(creado.Id, EstadoReporte.Resuelto, "Corregido"),
            CancellationToken.None);

        result.Estado.Should().Be(EstadoReporte.Resuelto);
        result.RespondidoPorUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task CambiarEstado_ARechazadoSinComentario_ShouldThrow()
    {
        await using var context = CreateContext();
        var creado = await new CreateReporteUsuarioCommandHandler(context, new TestUser("user-1"), new NullBitacora())
            .Handle(new CreateReporteUsuarioCommand(TipoReporte.Falla, "A", "Descripcion A"), CancellationToken.None);

        var handler = new CambiarEstadoReporteCommandHandler(context, new TestUser("admin-1"), new NullBitacora());
        var act = () => handler.Handle(
            new CambiarEstadoReporteCommand(creado.Id, EstadoReporte.Rechazado, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class TestUser(string id) : IUser
    {
        public string? Id => id;
        public string? Email => $"{id}@kpg.com";
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
