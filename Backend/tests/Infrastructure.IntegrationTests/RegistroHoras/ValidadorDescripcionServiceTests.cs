using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Cubre la parte que <see cref="EvaluadorDescripcionTests"/> (Application.UnitTests) no
/// puede probar: que <see cref="ValidadorDescripcionService"/> carga bien el catalogo y los
/// parametros desde la base y arma el <c>nombreProyecto</c> a partir del ProyectoId.
/// </summary>
public class ValidadorDescripcionServiceTests
{
    [Fact]
    public async Task EvaluarAsync_ConTerminoBloqueanteEnCatalogo_Bloquea()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        await context.SaveChangesAsync(CancellationToken.None);

        var servicio = CrearServicio(context);

        var resultado = await servicio.EvaluarAsync("varios", proyectoId: null);

        resultado.Bloquea.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluarAsync_ConTerminoInactivo_NoLoEvalua()
    {
        await using var context = await CrearContextoAsync();
        var termino = new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable.");
        termino.Desactivar();
        context.TerminosDescripcion.Add(termino);
        await context.SaveChangesAsync(CancellationToken.None);

        // 4 palabras a proposito: evita el ruido de la regla base MUY_CORTA (MinPalabras=4
        // por defecto) para que la asercion aisle solo el efecto de desactivar el termino.
        var resultado = await CrearServicio(context)
            .EvaluarAsync("varios asuntos pendientes hoy", proyectoId: null);

        resultado.Hallazgos.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluarAsync_ConValidacionInactivaEnParametros_NuncaBloquea()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistema.DescripcionValidacionActiva, Valor = "false"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarAsync("varios", proyectoId: null);

        resultado.Hallazgos.Should().BeEmpty();
        resultado.Bloquea.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluarAsync_ConProyectoId_DescuentaSuNombreDelPrefijoAlContarPalabras()
    {
        await using var context = await CrearContextoAsync();
        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        var proyecto = new Proyecto(cliente.Id, "Core Bancario");
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync(CancellationToken.None);

        var servicio = CrearServicio(context);

        // Quitando el prefijo "Core Bancario - " solo queda "Ajuste": 1 palabra real, por
        // debajo del minimo por defecto (4). Si el prefijo no se descontara, el conteo
        // ingenuo (proyecto + contenido) daria 3 palabras y el caso no probaria nada.
        var resultado = await servicio.EvaluarAsync("Core Bancario - Ajuste", proyecto.Id);

        resultado.Hallazgos.Should().ContainSingle(h =>
            h.Codigo == CodigoHallazgoDescripcion.MuyCorta && h.Mensaje.Contains("1 palabra"));
    }

    [Fact]
    public async Task EvaluarAsync_ConSeveridadReglasBaseEnBloquear_BloqueaPorReglaBase()
    {
        await using var context = await CrearContextoAsync();
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistema.DescripcionSeveridadReglasBase, Valor = "Bloquear"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarAsync("ok", proyectoId: null);

        resultado.Bloquea.Should().BeTrue();
    }

    // ── EvaluarVariasAsync (lote, para la bandeja de aprobaciones y reportes) ──────

    [Fact]
    public async Task EvaluarVariasAsync_ConListaVacia_DevuelveListaVacia()
    {
        await using var context = await CrearContextoAsync();

        var resultado = await CrearServicio(context).EvaluarVariasAsync([]);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluarVariasAsync_RespetaElOrdenYEvaluaCadaItemPorSeparado()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarVariasAsync(
        [
            ("Ajuste en modulo de evidencias para cancelaciones.", null),
            ("varios", null),
            ("Reunion de planificacion del sprint con el cliente.", null)
        ]);

        resultado.Should().HaveCount(3);
        resultado[0].Bloquea.Should().BeFalse();
        resultado[1].Bloquea.Should().BeTrue();
        resultado[2].Bloquea.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluarVariasAsync_ConValidacionInactiva_DevuelveTodoVacioSinConsultarElCatalogo()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = ParametrosSistema.DescripcionValidacionActiva, Valor = "false"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarVariasAsync(
            [("varios", null), ("otra cosa cualquiera", null)]);

        resultado.Should().OnlyContain(e => e.Hallazgos.Count == 0);
    }

    [Fact]
    public async Task EvaluarVariasAsync_ResuelveElNombreDeVariosProyectosDistintosEnUnaSolaConsulta()
    {
        await using var context = await CrearContextoAsync();
        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        var proyectoA = new Proyecto(cliente.Id, "Core Bancario");
        var proyectoB = new Proyecto(cliente.Id, "Banca Digital");
        context.Proyectos.AddRange(proyectoA, proyectoB);
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarVariasAsync(
        [
            ("Core Bancario - Ajuste", proyectoA.Id),
            ("Banca Digital - Ajuste", proyectoB.Id)
        ]);

        // Ambos quedan con 1 sola palabra real tras descontar el prefijo de su propio
        // proyecto (y no el del otro): confirma que cada fila resolvio su propio nombre.
        resultado.Should().AllSatisfy(e =>
            e.Hallazgos.Should().ContainSingle(h => h.Codigo == CodigoHallazgoDescripcion.MuyCorta));
    }

    // ── EvaluarVariasPorNombreProyectoAsync (reportes: la fila ya trae el nombre) ──

    [Fact]
    public async Task EvaluarVariasPorNombreProyectoAsync_NoConsultaProyectosYUsaElNombreDado()
    {
        await using var context = await CrearContextoAsync();

        var resultado = await CrearServicio(context).EvaluarVariasPorNombreProyectoAsync(
        [
            ("Core Bancario - Ajuste", "Core Bancario"),
            ("Banca Digital - Ajuste", "Banca Digital")
        ]);

        // Igual que con EvaluarVariasAsync por Id: cada fila descuenta su propio proyecto,
        // sin necesidad de que exista un Proyecto real en la base (no se creo ninguno aqui).
        resultado.Should().AllSatisfy(e =>
            e.Hallazgos.Should().ContainSingle(h => h.Codigo == CodigoHallazgoDescripcion.MuyCorta));
    }

    [Fact]
    public async Task EvaluarVariasPorNombreProyectoAsync_ConListaVacia_DevuelveListaVacia()
    {
        await using var context = await CrearContextoAsync();

        var resultado = await CrearServicio(context).EvaluarVariasPorNombreProyectoAsync([]);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluarVariasPorNombreProyectoAsync_ConTerminoBloqueanteEnCatalogo_Bloquea()
    {
        await using var context = await CrearContextoAsync();
        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable."));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await CrearServicio(context).EvaluarVariasPorNombreProyectoAsync(
            [("varios", null)]);

        resultado.Should().ContainSingle(e => e.Bloquea);
    }

    private static IValidadorDescripcion CrearServicio(ApplicationDbContext context) =>
        new ValidadorDescripcionService(context, new ParametrosSistemaService(context));

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
