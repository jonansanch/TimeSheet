using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class RegistroHorasImmutabilityTests
{
    private static readonly DateOnly Today = new(2026, 5, 14);

    [Fact]
    public async Task SaveChanges_WhenOnlyDescripcionChanges_Succeeds()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        registro.UpdateDescripcion("Descripcion actualizada");
        await context.SaveChangesAsync(CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(registro.Id);
        updated!.Descripcion.Should().Be("Descripcion actualizada");
    }

    [Fact]
    public async Task SaveChanges_WhenClienteChanges_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.Cliente)).CurrentValue = "Otro cliente";
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.Cliente)).IsModified = true;

        var act = () => context.SaveChangesAsync(CancellationToken.None);
        await act.Should().ThrowAsync<DomainRuleException>()
            .WithMessage("*inmutables*descripcion*");
    }

    [Fact]
    public async Task SaveChanges_WhenFechaRegistroChanges_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.FechaRegistro)).CurrentValue = Today.AddDays(-1);
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.FechaRegistro)).IsModified = true;

        var act = () => context.SaveChangesAsync(CancellationToken.None);
        await act.Should().ThrowAsync<DomainRuleException>();
    }

    [Fact]
    public async Task SaveChanges_WhenHoraEntradaChanges_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada)).CurrentValue = new TimeOnly(7, 30);
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada)).IsModified = true;

        var act = () => context.SaveChangesAsync(CancellationToken.None);
        await act.Should().ThrowAsync<DomainRuleException>();
    }

    [Fact]
    public async Task SaveChanges_WhenEsRetroactivoChanges_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.EsRetroactivo)).CurrentValue = true;
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.EsRetroactivo)).IsModified = true;

        var act = () => context.SaveChangesAsync(CancellationToken.None);
        await act.Should().ThrowAsync<DomainRuleException>();
    }

    [Fact]
    public async Task CreateRegistro_WhenFechaFueraVentanaConSolicitudAprobada_Succeeds()
    {
        var fechaFueraVentana = new DateOnly(2026, 5, 7);
        await using var context = CreateContextWithVentana(3);
        var solicitud = new SolicitudExcepcion("user-1", fechaFueraVentana, "Justificacion valida");
        solicitud.Aprobar();
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(Today));
        var result = await handler.Handle(CommandForDate(fechaFueraVentana), CancellationToken.None);

        result.FechaRegistro.Should().Be(fechaFueraVentana);
        result.EsRetroactivo.Should().BeTrue();
    }

    private static async Task<KPG.Timesheet.Domain.Entities.RegistroHoras> SeedRegistroAsync(ApplicationDbContext context)
    {
        var registro = CreateRegistro();
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);
        return registro;
    }

    private static KPG.Timesheet.Domain.Entities.RegistroHoras CreateRegistro() =>
        new(
            "user-1",
            Today,
            TurnoRegistro.AM,
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Descripcion original",
            "Bogota");

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new RegistroHorasImmutabilityInterceptor())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateContextWithVentana(int dias)
    {
        var context = CreateContext();
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = KPG.Timesheet.Domain.Constants.ParametrosSistema.VentanaRetroactividad,
            Valor = dias.ToString()
        });
        context.SaveChanges();
        return context;
    }

    private static CreateRegistroHorasCommand CommandForDate(DateOnly fecha) =>
        new(fecha, TurnoRegistro.AM,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
    }
}
