using FluentAssertions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.SolicitudesExcepcion;

public class CreateSolicitudExcepcionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateWithEstadoPendiente()
    {
        await using var context = CreateContext();
        var handler = new CreateSolicitudExcepcionCommandHandler(context, new TestUser("user-1"));
        var command = new CreateSolicitudExcepcionCommand(new DateOnly(2026, 4, 1), "Viaje de negocios imprevisto");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Estado.Should().Be("Pendiente");
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user-1");
        result.FechaRegistro.Should().Be(new DateOnly(2026, 4, 1));
        result.Justificacion.Should().Be("Viaje de negocios imprevisto");

        context.SolicitudesExcepcion.Should().ContainSingle(s => s.Id == result.Id);
    }

    [Fact]
    public async Task Handle_WhenJustificacionHasTrailingWhitespace_ShouldTrimAndSave()
    {
        await using var context = CreateContext();
        var handler = new CreateSolicitudExcepcionCommandHandler(context, new TestUser("user-1"));
        var command = new CreateSolicitudExcepcionCommand(new DateOnly(2026, 4, 1), "  Emergencia familiar  ");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Justificacion.Should().Be("Emergencia familiar");
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        await using var context = CreateContext();
        var handler = new CreateSolicitudExcepcionCommandHandler(context, new TestUser(null));
        var command = new CreateSolicitudExcepcionCommand(new DateOnly(2026, 4, 1), "Justificacion valida");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class TestUser : IUser
    {
        public TestUser(string? id) => Id = id;
        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
