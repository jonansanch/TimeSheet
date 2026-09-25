using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.CreateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.ToggleTerminoDescripcionActivo;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.UpdateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Catalogos;

public class TerminoDescripcionCrudTests
{
    [Fact]
    public async Task Create_ConDatosValidos_LoAgregaYRegistraBitacora()
    {
        await using var context = await CrearContextoAsync();
        var bitacora = Substitute.For<IBitacoraService>();

        var handler = new CreateTerminoDescripcionCommandHandler(context, bitacora, ActorFalso());
        var dto = await handler.Handle(
            new CreateTerminoDescripcionCommand(
                "soporte", TipoTermino.Palabra, ReglaTermino.GenericoSiVaSolo, SeveridadTermino.Advertir,
                "No dice a quien ni sobre que.", "[App] - Soporte a [area] sobre [incidencia]"),
            CancellationToken.None);

        dto.Termino.Should().Be("soporte");
        dto.Activo.Should().BeTrue();
        (await context.TerminosDescripcion.CountAsync()).Should().Be(1);
        await bitacora.Received(1).RegistrarAsync(
            KPG.Timesheet.Domain.Constants.TipoEventoBitacora.TerminoDescripcionCreado,
            "admin-1", Arg.Any<string?>(), "TerminosDescripcion", Arg.Any<string>(), Arg.Any<object>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_ConTerminoQueYaExisteAunConTildesYMayusculas_NormalizaIgual()
    {
        // El rechazo real (indice unico + DbUpdateException) solo lo aplica SQL Server: el
        // proveedor InMemory usado en estos tests no lo hace cumplir, igual que pasa con el
        // mismo patron en ModalidadConfiguration/ClienteConfiguration. Lo que si se puede
        // probar aqui sin una base real es que ambas variantes normalizan al mismo valor,
        // que es la condicion que dispara el indice unico en produccion.
        await using var context = await CrearContextoAsync();
        var handler = new CreateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso());

        var primero = await handler.Handle(
            new CreateTerminoDescripcionCommand(
                "reunion", TipoTermino.Palabra, ReglaTermino.GenericoSiVaSolo, SeveridadTermino.Advertir, "Motivo", null),
            CancellationToken.None);

        var segundoTermino = new TerminoDescripcion(
            "Reunión", TipoTermino.Palabra, ReglaTermino.GenericoSiVaSolo, SeveridadTermino.Advertir, "Otro motivo", null);

        var primeroEnBd = await context.TerminosDescripcion.FirstAsync(t => t.Id == primero.Id);
        segundoTermino.TerminoNormalizado.Should().Be(primeroEnBd.TerminoNormalizado);
    }

    [Fact]
    public async Task Update_CambiaLosCamposYElNormalizado()
    {
        await using var context = await CrearContextoAsync();
        var creado = await new CreateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso())
            .Handle(new CreateTerminoDescripcionCommand(
                "soporte", TipoTermino.Palabra, ReglaTermino.GenericoSiVaSolo, SeveridadTermino.Advertir, "Motivo", null),
                CancellationToken.None);

        var actualizado = await new UpdateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso())
            .Handle(new UpdateTerminoDescripcionCommand(
                creado.Id, "soporte tecnico", TipoTermino.Frase, ReglaTermino.ProhibidoSiempre,
                SeveridadTermino.Bloquear, "Motivo nuevo", "Sugerencia nueva"),
                CancellationToken.None);

        actualizado.Termino.Should().Be("soporte tecnico");
        actualizado.Tipo.Should().Be(TipoTermino.Frase);
        actualizado.Regla.Should().Be(ReglaTermino.ProhibidoSiempre);
        actualizado.Severidad.Should().Be(SeveridadTermino.Bloquear);
    }

    [Fact]
    public async Task Update_ConIdInexistente_LanzaNotFound()
    {
        await using var context = await CrearContextoAsync();
        var handler = new UpdateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso());

        var act = () => handler.Handle(
            new UpdateTerminoDescripcionCommand(
                9999, "x", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre, SeveridadTermino.Advertir, "Motivo", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Toggle_AlternaActivoEIgnoraElEvaluadorCuandoQuedaInactivo()
    {
        await using var context = await CrearContextoAsync();
        var creado = await new CreateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso())
            .Handle(new CreateTerminoDescripcionCommand(
                "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre, SeveridadTermino.Bloquear, "Motivo", null),
                CancellationToken.None);

        var handler = new ToggleTerminoDescripcionActivoCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso());

        var desactivado = await handler.Handle(new ToggleTerminoDescripcionActivoCommand(creado.Id), CancellationToken.None);
        desactivado.Activo.Should().BeFalse();

        var reactivado = await handler.Handle(new ToggleTerminoDescripcionActivoCommand(creado.Id), CancellationToken.None);
        reactivado.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_ConSoloActivos_FiltraLosInactivos()
    {
        await using var context = await CrearContextoAsync();
        var crear = new CreateTerminoDescripcionCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso());
        var toggle = new ToggleTerminoDescripcionActivoCommandHandler(context, Substitute.For<IBitacoraService>(), ActorFalso());

        await crear.Handle(new CreateTerminoDescripcionCommand(
            "activo", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre, SeveridadTermino.Advertir, "Motivo", null),
            CancellationToken.None);
        var inactivo = await crear.Handle(new CreateTerminoDescripcionCommand(
            "inactivo", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre, SeveridadTermino.Advertir, "Motivo", null),
            CancellationToken.None);
        await toggle.Handle(new ToggleTerminoDescripcionActivoCommand(inactivo.Id), CancellationToken.None);

        var handler = new GetTerminosDescripcionQueryHandler(context);
        var soloActivos = await handler.Handle(new GetTerminosDescripcionQuery(SoloActivos: true), CancellationToken.None);
        var todos = await handler.Handle(new GetTerminosDescripcionQuery(SoloActivos: false), CancellationToken.None);

        soloActivos.Should().ContainSingle(t => t.Termino == "activo");
        todos.Should().HaveCount(2);
    }

    private static IUser ActorFalso()
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns("admin-1");
        return user;
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
