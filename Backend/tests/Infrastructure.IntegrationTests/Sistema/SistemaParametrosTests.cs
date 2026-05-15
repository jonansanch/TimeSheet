using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateUmbralNotificacion;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateVentanaRetroactividad;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetUmbralNotificacion;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetVentanaRetroactividad;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Sistema;

public class SistemaParametrosTests
{
    // -------------------------------------------------------------------
    // GetVentanaRetroactividadQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetVentanaRetroactividad_WithParamExistente_ReturnsDias()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
        {
            Clave = ParametrosSistema.VentanaRetroactividad,
            Valor = "5"
        });
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new GetVentanaRetroactividadQueryHandler(context);

        var resultado = await handler.Handle(new GetVentanaRetroactividadQuery(), CancellationToken.None);

        resultado.Should().Be(5);
    }

    [Fact]
    public async Task GetVentanaRetroactividad_SinParametro_ReturnsDefault3()
    {
        await using var context = CreateContext();
        var handler = new GetVentanaRetroactividadQueryHandler(context);

        var resultado = await handler.Handle(new GetVentanaRetroactividadQuery(), CancellationToken.None);

        resultado.Should().Be(3);
    }

    // -------------------------------------------------------------------
    // UpdateVentanaRetroactividadCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateVentanaRetroactividad_WithParamExistente_ActualizaValor()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
        {
            Clave = ParametrosSistema.VentanaRetroactividad,
            Valor = "3"
        });
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new UpdateVentanaRetroactividadCommandHandler(context);

        await handler.Handle(new UpdateVentanaRetroactividadCommand(7), CancellationToken.None);

        var param = context.ParametrosSistema.First(p => p.Clave == ParametrosSistema.VentanaRetroactividad);
        param.Valor.Should().Be("7");
    }

    [Fact]
    public async Task UpdateVentanaRetroactividad_SinParametro_CreaParametro()
    {
        await using var context = CreateContext();
        var handler = new UpdateVentanaRetroactividadCommandHandler(context);

        await handler.Handle(new UpdateVentanaRetroactividadCommand(5), CancellationToken.None);

        var param = context.ParametrosSistema.FirstOrDefault(p => p.Clave == ParametrosSistema.VentanaRetroactividad);
        param.Should().NotBeNull();
        param!.Valor.Should().Be("5");
    }

    // -------------------------------------------------------------------
    // GetUmbralNotificacionQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetUmbralNotificacion_WithParamExistente_ReturnsDias()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
        {
            Clave = ParametrosSistema.DiasUmbralNotificacion,
            Valor = "5"
        });
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new GetUmbralNotificacionQueryHandler(context);

        var resultado = await handler.Handle(new GetUmbralNotificacionQuery(), CancellationToken.None);

        resultado.Should().Be(5);
    }

    [Fact]
    public async Task GetUmbralNotificacion_SinParametro_ReturnsDefault3()
    {
        await using var context = CreateContext();
        var handler = new GetUmbralNotificacionQueryHandler(context);

        var resultado = await handler.Handle(new GetUmbralNotificacionQuery(), CancellationToken.None);

        resultado.Should().Be(3);
    }

    // -------------------------------------------------------------------
    // UpdateUmbralNotificacionCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateUmbralNotificacion_WithParamExistente_ActualizaValor()
    {
        await using var context = CreateContext();
        context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
        {
            Clave = ParametrosSistema.DiasUmbralNotificacion,
            Valor = "3"
        });
        await context.SaveChangesAsync(CancellationToken.None);
        var handler = new UpdateUmbralNotificacionCommandHandler(context);

        await handler.Handle(new UpdateUmbralNotificacionCommand(10), CancellationToken.None);

        var param = context.ParametrosSistema.First(p => p.Clave == ParametrosSistema.DiasUmbralNotificacion);
        param.Valor.Should().Be("10");
    }

    [Fact]
    public async Task UpdateUmbralNotificacion_SinParametro_CreaParametro()
    {
        await using var context = CreateContext();
        var handler = new UpdateUmbralNotificacionCommandHandler(context);

        await handler.Handle(new UpdateUmbralNotificacionCommand(7), CancellationToken.None);

        var param = context.ParametrosSistema.FirstOrDefault(p => p.Clave == ParametrosSistema.DiasUmbralNotificacion);
        param.Should().NotBeNull();
        param!.Valor.Should().Be("7");
    }

    // -------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
