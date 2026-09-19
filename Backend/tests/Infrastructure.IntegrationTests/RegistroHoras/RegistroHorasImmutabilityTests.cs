using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
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
    public async Task SaveChanges_WhenModalidadChanges_Succeeds()
    {
        // Metadata del dia (Modalidad, Recurso, Lugar) sigue siendo mutable via UpdateMetadata.
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.Modalidad)).CurrentValue = "Cliente";
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.Modalidad)).IsModified = true;

        await context.SaveChangesAsync(CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(registro.Id);
        updated!.Modalidad.Should().Be("Cliente");
    }

    [Fact]
    public async Task SaveChanges_WhenHoraEntrada1ChangedFromNull_Succeeds()
    {
        // SoloAgregable: null → value is allowed (adding a missing turno)
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context); // AM-only registro, PM is null

        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada2)).CurrentValue = new TimeOnly(13, 0);
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada2)).IsModified = true;

        await context.SaveChangesAsync(CancellationToken.None);

        var updated = await context.RegistrosHoras.FindAsync(registro.Id);
        updated!.HoraEntrada2.Should().Be(new TimeOnly(13, 0));
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
    public async Task SaveChanges_WhenHoraEntrada1ChangedFromValue_ThrowsDomainRuleException()
    {
        await using var context = CreateContext();
        var registro = await SeedRegistroAsync(context);

        // Trying to change an already-set AM time is forbidden (value → value)
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada1)).CurrentValue = new TimeOnly(7, 30);
        context.Entry(registro).Property(nameof(KPG.Timesheet.Domain.Entities.RegistroHoras.HoraEntrada1)).IsModified = true;

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

        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(Today), new NullBitacora(), VentanaService(context));
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
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            null,
            null,
            null, null,
            1,
            "Cliente 1",
            "Proyecto 1",
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
        var context = new ApplicationDbContext(options);
        CatalogoDePrueba.SembrarAsync(context).GetAwaiter().GetResult();
        return context;
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
        new(fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            1, "Remoto", "Consultor", "Desarrollo", "Bogota");

    /// <summary>
    /// Servicio real, no un doble: la ventana efectiva depende del parametro global y de
    /// las reglas por persona o rol, y eso es parte de lo que estos tests ejercitan.
    /// </summary>
    private static VentanaRetroactividadService VentanaService(ApplicationDbContext context) =>
        new(context, new ParametrosSistemaService(context));

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
        public DateTimeOffset UtcNow => Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
