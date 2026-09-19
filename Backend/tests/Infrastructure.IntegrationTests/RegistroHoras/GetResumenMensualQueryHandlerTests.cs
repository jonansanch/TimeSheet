using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetResumenMensual;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class GetResumenMensualQueryHandlerTests
{
    private static readonly DateOnly Mayo10 = new(2026, 5, 10);

    [Fact]
    public async Task Handle_WhenUserHasNoRegistros_ShouldReturnEmptyDaysWithThreshold()
    {
        await using var context = CreateContext();

        var result = await Handle(context, "user-sin-registros");

        result.Dias.Should().BeEmpty();
        result.MinutosDiaCompleto.Should().Be(480);
    }

    [Fact]
    public async Task Handle_ShouldSumAllThreeBlocksOfTheDay()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro(
            "user-1", Mayo10, 1,
            (new TimeOnly(8, 0),  new TimeOnly(12, 0)),    // 240
            (new TimeOnly(13, 0), new TimeOnly(17, 0)),    // 240
            (new TimeOnly(19, 0), new TimeOnly(19, 30)))); //  30
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.Dias.Should().ContainSingle()
            .Which.TotalMinutos.Should().Be(510);
    }

    [Fact]
    public async Task Handle_WhenDayHasSeveralProjects_ShouldSumThemIntoOneDay()
    {
        // Un dia puede tener un registro por proyecto: el calendario evalua el total del dia.
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro(
            "user-1", Mayo10, 1,
            (new TimeOnly(8, 0), new TimeOnly(12, 0)), null, null));
        context.RegistrosHoras.Add(MakeRegistro(
            "user-1", Mayo10, 2,
            (new TimeOnly(13, 0), new TimeOnly(17, 0)), null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.Dias.Should().ContainSingle()
            .Which.TotalMinutos.Should().Be(480);
    }

    [Fact]
    public async Task Handle_ShouldIgnoreOtherUsersAndOtherMonths()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", Mayo10, 3,
            (new TimeOnly(8, 0), new TimeOnly(12, 0)), null, null));
        context.RegistrosHoras.Add(MakeRegistro("user-2", Mayo10, 4,
            (new TimeOnly(8, 0), new TimeOnly(12, 0)), null, null));
        context.RegistrosHoras.Add(MakeRegistro("user-1", new DateOnly(2026, 6, 3), 5,
            (new TimeOnly(8, 0), new TimeOnly(12, 0)), null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.Dias.Should().ContainSingle()
            .Which.Fecha.Should().Be(Mayo10);
    }

    [Fact]
    public async Task Handle_ShouldUseConfiguredThreshold()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistemaKeys.HorasDiaCompleto,
            Valor = "6"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.MinutosDiaCompleto.Should().Be(360);
    }

    [Fact]
    public async Task Handle_WhenThresholdIsNotPositive_ShouldFallBackToEightHours()
    {
        // Un umbral de 0 marcaria como completo cualquier dia con un minuto registrado.
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistemaKeys.HorasDiaCompleto,
            Valor = "0"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.MinutosDiaCompleto.Should().Be(480);
    }

    [Fact]
    public async Task Handle_WhenThresholdIsNotANumber_ShouldFallBackToEightHours()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistemaKeys.HorasDiaCompleto,
            Valor = "ocho"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Handle(context, "user-1");

        result.MinutosDiaCompleto.Should().Be(480);
    }

    private static Task<ResumenMensualResponse> Handle(ApplicationDbContext context, string userId)
    {
        var handler = new GetResumenMensualQueryHandler(
            context, new TestUser(userId), new ParametrosSistemaService(context));
        return handler.Handle(new GetResumenMensualQuery(5, 2026), CancellationToken.None);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static RegistroHorasEntity MakeRegistro(
        string userId,
        DateOnly fecha,
        int proyectoId,
        (TimeOnly Entrada, TimeOnly Salida)? bloque1,
        (TimeOnly Entrada, TimeOnly Salida)? bloque2,
        (TimeOnly Entrada, TimeOnly Salida)? bloque3) =>
        new(userId, fecha,
            bloque1?.Entrada, bloque1?.Salida,
            bloque2?.Entrada, bloque2?.Salida,
            bloque3?.Entrada, bloque3?.Salida,
            proyectoId, $"Cliente {proyectoId}", $"Proyecto {proyectoId}", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
