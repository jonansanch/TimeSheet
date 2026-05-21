using FluentAssertions;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using RegistroHoras = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class UpdateDescripcionRegistroHorasCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRegistroExiste_ActualizaDescripcionEnDb()
    {
        await using var context = CreateContext();
        var registro = CreateRegistro();
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateDescripcionRegistroHorasCommandHandler(context, Substitute.For<IBitacoraService>(), Substitute.For<IUser>());
        await handler.Handle(
            new UpdateDescripcionRegistroHorasCommand(registro.Id, "Descripción actualizada"),
            CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(registro.Id);
        updated!.Descripcion.Should().Be("Descripción actualizada");
    }

    [Fact]
    public async Task Handle_WhenRegistroNoExiste_LanzaNotFoundException()
    {
        await using var context = CreateContext();

        var handler = new UpdateDescripcionRegistroHorasCommandHandler(context, Substitute.For<IBitacoraService>(), Substitute.For<IUser>());
        var act = () => handler.Handle(
            new UpdateDescripcionRegistroHorasCommand(999, "Nueva descripción"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static KPG.Timesheet.Domain.Entities.RegistroHoras CreateRegistro() =>
        new(
            "user-1",
            new DateOnly(2026, 5, 14),
            TurnoRegistro.AM,
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Descripción original",
            "Bogota");

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
