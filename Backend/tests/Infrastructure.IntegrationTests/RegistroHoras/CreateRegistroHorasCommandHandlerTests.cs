using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class CreateRegistroHorasCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldPersistRegistroForAuthenticatedUser()
    {
        await using var context = CreateContext();
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user-1");
        context.RegistrosHoras.Should().ContainSingle(r =>
            r.UserId == "user-1" &&
            r.FechaRegistro == new DateOnly(2026, 5, 14) &&
            r.Turno == TurnoRegistro.AM);
    }

    [Fact]
    public async Task Handle_WhenDuplicateUserDateTurno_ShouldThrowValidationException()
    {
        await using var context = CreateContext();
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"));

        await handler.Handle(ValidCommand(), CancellationToken.None);
        var act = () => handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<KPG.Timesheet.Application.Common.Exceptions.ValidationException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(
            new DateOnly(2026, 5, 14),
            TurnoRegistro.AM,
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id)
        {
            Id = id;
        }

        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
