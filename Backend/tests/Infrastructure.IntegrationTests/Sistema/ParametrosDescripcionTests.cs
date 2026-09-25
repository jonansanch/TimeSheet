using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateParametrosDescripcion;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetParametrosDescripcion;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Sistema;

public class ParametrosDescripcionTests
{
    [Fact]
    public async Task Get_SinParametrosGuardados_DevuelveLosValoresPorDefecto()
    {
        await using var context = await CrearContextoAsync();

        var resultado = await new GetParametrosDescripcionQueryHandler(new ParametrosSistemaService(context))
            .Handle(new GetParametrosDescripcionQuery(), CancellationToken.None);

        var defecto = ParametrosEvaluacionDescripcion.PorDefecto;
        resultado.ValidacionActiva.Should().Be(defecto.ValidacionActiva);
        resultado.MinPalabras.Should().Be(defecto.MinPalabras);
        resultado.MinPalabrasContexto.Should().Be(defecto.MinPalabrasContexto);
        resultado.SeveridadReglasBase.Should().Be(defecto.SeveridadReglasBase);
    }

    [Fact]
    public async Task Update_LuegoGet_DevuelveLosValoresGuardadosYRegistraBitacora()
    {
        await using var context = await CrearContextoAsync();
        var bitacora = Substitute.For<IBitacoraService>();
        var actor = Substitute.For<IUser>();
        actor.Id.Returns("admin-1");

        await new UpdateParametrosDescripcionCommandHandler(context, bitacora, actor).Handle(
            new UpdateParametrosDescripcionCommand(false, 6, 2, SeveridadTermino.Bloquear),
            CancellationToken.None);

        var resultado = await new GetParametrosDescripcionQueryHandler(new ParametrosSistemaService(context))
            .Handle(new GetParametrosDescripcionQuery(), CancellationToken.None);

        resultado.ValidacionActiva.Should().BeFalse();
        resultado.MinPalabras.Should().Be(6);
        resultado.MinPalabrasContexto.Should().Be(2);
        resultado.SeveridadReglasBase.Should().Be(SeveridadTermino.Bloquear);

        await bitacora.Received(1).RegistrarAsync(
            KPG.Timesheet.Domain.Constants.TipoEventoBitacora.CambioParametrosDescripcion,
            "admin-1", Arg.Any<string?>(), "ParametrosSistema", Arg.Any<string?>(), Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_LlamadoDosVeces_SobrescribeEnVezDeDuplicar()
    {
        await using var context = await CrearContextoAsync();
        var actor = Substitute.For<IUser>();
        actor.Id.Returns("admin-1");
        var handler = new UpdateParametrosDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), actor);

        await handler.Handle(new UpdateParametrosDescripcionCommand(true, 4, 3, SeveridadTermino.Advertir), CancellationToken.None);
        await handler.Handle(new UpdateParametrosDescripcionCommand(true, 8, 5, SeveridadTermino.Bloquear), CancellationToken.None);

        (await context.ParametrosSistema.CountAsync(p => p.Clave == KPG.Timesheet.Domain.Constants.ParametrosSistema.DescripcionMinPalabras))
            .Should().Be(1);

        var resultado = await new GetParametrosDescripcionQueryHandler(new ParametrosSistemaService(context))
            .Handle(new GetParametrosDescripcionQuery(), CancellationToken.None);
        resultado.MinPalabras.Should().Be(8);
    }

    private static async Task<ApplicationDbContext> CrearContextoAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
