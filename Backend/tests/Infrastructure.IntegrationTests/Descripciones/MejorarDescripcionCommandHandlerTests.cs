using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.Descripciones;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Descripciones;

public class MejorarDescripcionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ConRedactorNoDisponible_DevuelveDisponibleFalseYNoLlamaAlRedactor()
    {
        await using var context = await CrearContextoAsync();
        var redactor = RedactorFalso(disponible: false);

        var resultado = await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("varios", null), CancellationToken.None);

        resultado.Disponible.Should().BeFalse();
        resultado.Propuesta.Should().BeNull();
        await redactor.DidNotReceive().MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConPropuestaDelModelo_LaDevuelveYLaReevaluaConElCatalogo()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        await context.SaveChangesAsync(CancellationToken.None);

        var redactor = RedactorFalso(disponible: true);
        redactor.MejorarAsync(
            "varios", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new PropuestaDescripcionDto(
                "CIMA - Ajuste en modulo de evidencias.", ["Se aplico el formato de la guia."]));

        var resultado = await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("varios", null), CancellationToken.None);

        resultado.Disponible.Should().BeTrue();
        resultado.Propuesta.Should().Be("CIMA - Ajuste en modulo de evidencias.");
        resultado.Cambios.Should().ContainSingle();
        // La propuesta ya no contiene el termino bloqueante: no deberia quedar bloqueada.
        resultado.PropuestaBloquea.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConPropuestaQueSigueTeniendoUnTerminoBloqueante_LoInformaEnLaRespuesta()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        await context.SaveChangesAsync(CancellationToken.None);

        var redactor = RedactorFalso(disponible: true);
        redactor.MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new PropuestaDescripcionDto("Se hicieron varios ajustes.", []));

        var resultado = await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("varios", null), CancellationToken.None);

        resultado.PropuestaBloquea.Should().BeTrue();
        resultado.ObservacionesPropuesta.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ConElModeloSinPropuesta_DevuelvePropuestaNulaSinFallar()
    {
        await using var context = await CrearContextoAsync();
        var redactor = RedactorFalso(disponible: true);
        redactor.MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(PropuestaDescripcionDto.Vacia);

        var resultado = await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("Ajuste en modulo de evidencias.", null), CancellationToken.None);

        resultado.Disponible.Should().BeTrue();
        resultado.Propuesta.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RegistraElUsoEnBitacora()
    {
        await using var context = await CrearContextoAsync();
        var redactor = RedactorFalso(disponible: true);
        redactor.MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new PropuestaDescripcionDto("Texto mejorado.", []));

        await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("texto original", null), CancellationToken.None);

        (await context.BitacoraAuditoria.CountAsync(
            b => b.TipoEvento == TipoEventoBitacora.MejoraDescripcionSolicitada && b.ActorId == "user-1"))
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_AlAlcanzarElLimiteDiario_LanzaValidationExceptionYNoLlamaAlRedactor()
    {
        await using var context = await CrearContextoAsync();
        var redactor = RedactorFalso(disponible: true, limiteDiario: 2);

        for (var i = 0; i < 2; i++)
        {
            context.BitacoraAuditoria.Add(BitacoraAuditoria.Crear(
                TipoEventoBitacora.MejoraDescripcionSolicitada, "user-1", null,
                "RegistrosHoras", null, null, DateTimeOffset.UtcNow));
        }
        await context.SaveChangesAsync(CancellationToken.None);

        var act = () => CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("texto", null), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await redactor.DidNotReceive().MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConUsosDeAyer_NoCuentanParaElLimiteDeHoy()
    {
        await using var context = await CrearContextoAsync();
        var redactor = RedactorFalso(disponible: true, limiteDiario: 1);

        context.BitacoraAuditoria.Add(BitacoraAuditoria.Crear(
            TipoEventoBitacora.MejoraDescripcionSolicitada, "user-1", null,
            "RegistrosHoras", null, null, DateTimeOffset.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync(CancellationToken.None);

        redactor.MejorarAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(new PropuestaDescripcionDto("Texto mejorado.", []));

        var resultado = await CrearHandler(context, redactor).Handle(
            new MejorarDescripcionCommand("texto", null), CancellationToken.None);

        resultado.Propuesta.Should().Be("Texto mejorado.");
    }

    private static IRedactorDescripcion RedactorFalso(bool disponible, int limiteDiario = 20)
    {
        var redactor = Substitute.For<IRedactorDescripcion>();
        redactor.Disponible.Returns(disponible);
        redactor.LimiteDiarioPorUsuario.Returns(limiteDiario);
        return redactor;
    }

    private static MejorarDescripcionCommandHandler CrearHandler(ApplicationDbContext context, IRedactorDescripcion redactor)
    {
        var actor = Substitute.For<IUser>();
        actor.Id.Returns("user-1");

        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        return new MejorarDescripcionCommandHandler(
            redactor,
            new ValidadorDescripcionService(context, new ParametrosSistemaService(context)),
            context,
            new BitacoraServiceFake(context),
            clock,
            actor);
    }

    /// <summary>
    /// <see cref="BitacoraService"/> real depende de <c>TimeProvider</c> ademas del contexto;
    /// para no arrastrar esa dependencia aqui se usa un doble minimo que hace lo mismo:
    /// agregar la entrada al contexto, sin guardar (igual que el real).
    /// </summary>
    private sealed class BitacoraServiceFake(ApplicationDbContext context) : IBitacoraService
    {
        public Task RegistrarAsync(
            string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default)
        {
            context.BitacoraAuditoria.Add(BitacoraAuditoria.Crear(
                tipoEvento, actorId, actorEmail, entidadAfectada, entidadId, metadata, DateTimeOffset.UtcNow));
            return Task.CompletedTask;
        }
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
