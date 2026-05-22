using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Infrastructure.Bitacora;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Bitacora;

public class BitacoraServiceTests
{
    [Fact]
    public async Task RegistrarAsync_GuardaEntradaEnBD()
    {
        var (context, service) = CreateService();

        await service.RegistrarAsync(
            TipoEventoBitacora.LoginExitoso,
            "user-123", "user@kpg.com",
            "AspNetUsers", "user-123",
            new { Email = "user@kpg.com" });
        await context.SaveChangesAsync();

        var entradas = await context.BitacoraAuditoria.ToListAsync();
        entradas.Should().HaveCount(1);
        entradas[0].TipoEvento.Should().Be(TipoEventoBitacora.LoginExitoso);
        entradas[0].ActorId.Should().Be("user-123");
        entradas[0].ActorEmail.Should().Be("user@kpg.com");
        entradas[0].EntidadAfectada.Should().Be("AspNetUsers");
        entradas[0].EntidadId.Should().Be("user-123");
    }

    [Fact]
    public async Task RegistrarAsync_ConMetadata_SerializaJson()
    {
        var (context, service) = CreateService();

        await service.RegistrarAsync(
            TipoEventoBitacora.CambioRol,
            "admin-id", null,
            "AspNetUsers", "target-id",
            new { NuevoRol = "Supervisor" });
        await context.SaveChangesAsync();

        var entrada = await context.BitacoraAuditoria.FirstAsync();
        entrada.MetadataJson.Should().NotBeNull();
        entrada.MetadataJson.Should().Contain("NuevoRol");
        entrada.MetadataJson.Should().Contain("Supervisor");
    }

    [Fact]
    public async Task RegistrarAsync_SinMetadata_MetadataJsonEsNull()
    {
        var (context, service) = CreateService();

        await service.RegistrarAsync(
            TipoEventoBitacora.BajaUsuario,
            "admin-id", null,
            "AspNetUsers", "target-id");
        await context.SaveChangesAsync();

        var entrada = await context.BitacoraAuditoria.FirstAsync();
        entrada.MetadataJson.Should().BeNull();
    }

    [Fact]
    public async Task RegistrarAsync_TimestampEsUtc()
    {
        var (context, service) = CreateService();
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        await service.RegistrarAsync(
            TipoEventoBitacora.AltaUsuario,
            "admin-id", null,
            "AspNetUsers", "new-user");
        await context.SaveChangesAsync();

        var despues = DateTimeOffset.UtcNow.AddSeconds(1);
        var entrada = await context.BitacoraAuditoria.FirstAsync();
        entrada.Timestamp.Should().BeAfter(antes).And.BeBefore(despues);
    }

    [Fact]
    public async Task RegistrarAsync_VariosEventos_TodosPersistidosEnUnSaveChanges()
    {
        var (context, service) = CreateService();

        await service.RegistrarAsync(TipoEventoBitacora.LoginExitoso, "u1", "u1@kpg.com", "AspNetUsers", "u1");
        await service.RegistrarAsync(TipoEventoBitacora.CambioRol, "admin", null, "AspNetUsers", "u1");
        await service.RegistrarAsync(TipoEventoBitacora.BajaUsuario, "admin", null, "AspNetUsers", "u2");
        await context.SaveChangesAsync();

        var entradas = await context.BitacoraAuditoria.ToListAsync();
        entradas.Should().HaveCount(3);
        entradas.Select(e => e.TipoEvento).Should().Contain([
            TipoEventoBitacora.LoginExitoso,
            TipoEventoBitacora.CambioRol,
            TipoEventoBitacora.BajaUsuario
        ]);
    }

    private static (ApplicationDbContext context, IBitacoraService service) CreateService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IBitacoraService, BitacoraService>();

        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var bitacora = provider.GetRequiredService<IBitacoraService>();

        return (context, bitacora);
    }
}
