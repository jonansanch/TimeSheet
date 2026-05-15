using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.DeleteRegistroHoras;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class DeleteRegistroHorasCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRegistroExists_AndBelongsToUser_ShouldDeleteIt()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);
        var id = registro.Id;

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUser("user-1"));
        await handler.Handle(new DeleteRegistroHorasCommand(id), CancellationToken.None);

        context.RegistrosHoras.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRegistroNotFound_ShouldThrowNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUser("user-1"));

        var act = () => handler.Handle(new DeleteRegistroHorasCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRegistroBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-2", "KPG", "Timesheet", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUser("user-1"));

        var act = () => handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenDeleted_ShouldNotAppearInSubsequentQuery()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);
        var id = registro.Id;

        var deleteHandler = new DeleteRegistroHorasCommandHandler(context, new TestUser("user-1"));
        await deleteHandler.Handle(new DeleteRegistroHorasCommand(id), CancellationToken.None);

        var remaining = await context.RegistrosHoras
            .Where(r => r.UserId == "user-1")
            .ToListAsync(CancellationToken.None);
        remaining.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static RegistroHorasEntity MakeRegistro(
        string userId, string cliente, string proyecto, DateOnly fecha,
        TurnoRegistro turno = TurnoRegistro.AM) =>
        new(userId, fecha, turno,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            cliente, proyecto, "Remoto", "Consultor", "Desarrollo", "Bogota");

    [Fact]
    public async Task Handle_WhenAdminEliminaRegistroAjeno_Succeeds()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 14));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUserWithRol("user-2", KPG.Timesheet.Domain.Constants.Roles.Admin));
        await handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        var deleted = await context.RegistrosHoras.FindAsync(registro.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSupervisorEliminaRegistroAjeno_Succeeds()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 14));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUserWithRol("user-2", KPG.Timesheet.Domain.Constants.Roles.Supervisor));
        await handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        var deleted = await context.RegistrosHoras.FindAsync(registro.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEmpleadoEliminaRegistroAjeno_ThrowsForbiddenAccessException()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 14));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUserWithRol("user-2", KPG.Timesheet.Domain.Constants.Roles.Empleado));
        var act = () => handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestUserWithRol : IUser
    {
        public TestUserWithRol(string id, string rol)
        {
            Id = id;
            Roles = [rol];
        }

        public string? Id { get; }
        public List<string>? Roles { get; }
    }
}
