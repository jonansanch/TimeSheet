using FluentAssertions;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Common.Services;

public class EvaluadorDescripcionTests
{
    private static readonly ParametrosEvaluacionDescripcion Parametros = ParametrosEvaluacionDescripcion.PorDefecto;

    private static TerminoDescripcion Termino(
        string termino,
        TipoTermino tipo = TipoTermino.Palabra,
        ReglaTermino regla = ReglaTermino.ProhibidoSiempre,
        SeveridadTermino severidad = SeveridadTermino.Bloquear,
        string? sugerencia = "Sugerencia de ejemplo") =>
        new(termino, tipo, regla, severidad, "Motivo de ejemplo", sugerencia);

    // ── Interruptor general ─────────────────────────────────────────────────

    [Fact]
    public void Evaluar_ConValidacionInactiva_NuncaMarcaNada()
    {
        var parametros = Parametros with { ValidacionActiva = false };
        var terminos = new[] { Termino("varios") };

        var resultado = EvaluadorDescripcion.Evaluar("varios", terminos, parametros);

        resultado.Hallazgos.Should().BeEmpty();
        resultado.Bloquea.Should().BeFalse();
    }

    // ── Coincidencia por palabra completa, tildes y mayusculas ──────────────

    [Fact]
    public void Evaluar_TerminoProhibido_MarcaAunqueTengaTildesYMayusculas()
    {
        var terminos = new[] { Termino("reunion", regla: ReglaTermino.ProhibidoSiempre) };

        var resultado = EvaluadorDescripcion.Evaluar("Reunión", terminos, Parametros);

        resultado.Bloquea.Should().BeTrue();
        resultado.Hallazgos.Should().ContainSingle(h => h.Fragmento == "reunion");
    }

    [Fact]
    public void Evaluar_TerminoDentroDeOtraPalabra_NoCoincide()
    {
        // "soporte" no debe marcar "soportes tecnicos", son palabras distintas.
        var terminos = new[] { Termino("soporte", regla: ReglaTermino.ProhibidoSiempre) };

        var resultado = EvaluadorDescripcion.Evaluar(
            "CIMA - Ajuste en soportes tecnicos del modulo de pagos", terminos, Parametros);

        resultado.Hallazgos.Should().BeEmpty();
    }

    [Fact]
    public void Evaluar_TerminoComoPalabraCompleta_Coincide()
    {
        var terminos = new[] { Termino("soporte", regla: ReglaTermino.ProhibidoSiempre) };

        var resultado = EvaluadorDescripcion.Evaluar("Soporte", terminos, Parametros);

        resultado.Hallazgos.Should().ContainSingle(h => h.Fragmento == "soporte");
    }

    // ── GenericoSiVaSolo: contexto ───────────────────────────────────────────

    [Fact]
    public void Evaluar_GenericoSiVaSolo_SinContexto_Marca()
    {
        var terminos = new[] { Termino("reunion", regla: ReglaTermino.GenericoSiVaSolo) };

        var resultado = EvaluadorDescripcion.Evaluar("Reunion", terminos, Parametros);

        resultado.Hallazgos.Should().ContainSingle(h => h.Fragmento == "reunion");
    }

    [Fact]
    public void Evaluar_GenericoSiVaSolo_ConSuficienteContexto_NoMarca()
    {
        var terminos = new[] { Termino("reunion", regla: ReglaTermino.GenericoSiVaSolo) };

        var resultado = EvaluadorDescripcion.Evaluar(
            "Reunion de planificacion del sprint con el cliente", terminos, Parametros);

        resultado.Hallazgos.Should().BeEmpty();
    }

    [Fact]
    public void Evaluar_GenericoSiVaSolo_ElNombreDelProyectoNoCuentaComoContexto()
    {
        var terminos = new[] { Termino("reunion", regla: ReglaTermino.GenericoSiVaSolo) };

        // Sin el nombre del proyecto no queda nada de contexto: debe marcar igual que
        // "Reunion" sola.
        var resultado = EvaluadorDescripcion.Evaluar(
            "CIMA WebClient - Reunion", terminos, Parametros, nombreProyecto: "CIMA WebClient");

        resultado.Hallazgos.Should().ContainSingle(h => h.Fragmento == "reunion");
    }

    // ── Reglas base ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Prueba 1")]
    [InlineData("Test 2")]
    [InlineData("ok")]
    public void Evaluar_TextoMuyCorto_MarcaMuyCorta(string texto)
    {
        var resultado = EvaluadorDescripcion.Evaluar(texto, [], Parametros);

        resultado.Hallazgos.Should().Contain(h => h.Codigo == CodigoHallazgoDescripcion.MuyCorta);
    }

    [Fact]
    public void Evaluar_TextoConSuficientesPalabras_NoMarcaMuyCorta()
    {
        var resultado = EvaluadorDescripcion.Evaluar(
            "Analisis y desarrollo de modulo de pagos", [], Parametros);

        resultado.Hallazgos.Should().NotContain(h => h.Codigo == CodigoHallazgoDescripcion.MuyCorta);
    }

    [Fact]
    public void Evaluar_ConPrefijoDeProyecto_NoCuentaElPrefijoComoPalabras()
    {
        // "CIMA WebClient - Ajuste" son 4 palabras reales de la tarea si se descuenta el
        // prefijo del proyecto (que no es contenido del usuario).
        var conPrefijo = EvaluadorDescripcion.Evaluar(
            "CIMA WebClient - Ajuste", [], Parametros, nombreProyecto: "CIMA WebClient");

        conPrefijo.Hallazgos.Should().Contain(h => h.Codigo == CodigoHallazgoDescripcion.MuyCorta);
    }

    [Fact]
    public void Evaluar_TextoTodoEnMayusculas_MarcaMayusculas()
    {
        var resultado = EvaluadorDescripcion.Evaluar(
            "AJUSTE EN MODULO DE EVIDENCIAS PARA CANCELACIONES", [], Parametros);

        resultado.Hallazgos.Should().Contain(h => h.Codigo == CodigoHallazgoDescripcion.Mayusculas);
    }

    [Fact]
    public void Evaluar_TextoNormal_NoMarcaMayusculas()
    {
        var resultado = EvaluadorDescripcion.Evaluar(
            "CIMA WebClient - Ajuste en modulo de evidencias", [], Parametros);

        resultado.Hallazgos.Should().NotContain(h => h.Codigo == CodigoHallazgoDescripcion.Mayusculas);
    }

    [Theory]
    [InlineData("pruebas internas cccc")]
    [InlineData("aaaaaaaa modulo de pagos")]
    [InlineData("reunionnnnn con el cliente")]
    public void Evaluar_CaracteresRepetidos_Marca(string texto)
    {
        var resultado = EvaluadorDescripcion.Evaluar(texto, [], Parametros);

        resultado.Hallazgos.Should().Contain(h => h.Codigo == CodigoHallazgoDescripcion.CaracteresRepetidos);
    }

    [Fact]
    public void Evaluar_SinLetras_MarcaSinLetrasYNoEvaluaLasDemasReglas()
    {
        // "...." no tiene ninguna letra: SIN_LETRAS ya lo cubre, no hace falta ademas
        // marcarlo como muy corto o con caracteres repetidos.
        var resultado = EvaluadorDescripcion.Evaluar("....", [], Parametros);

        resultado.Hallazgos.Should().ContainSingle(h => h.Codigo == CodigoHallazgoDescripcion.SinLetras);
    }

    [Fact]
    public void Evaluar_ConMarcadorSinCompletar_Marca()
    {
        var resultado = EvaluadorDescripcion.Evaluar(
            "CIMA - Ajuste en [modulo/elemento] - resultado pendiente", [], Parametros);

        resultado.Hallazgos.Should().Contain(h => h.Codigo == CodigoHallazgoDescripcion.MarcadorSinLlenar);
    }

    [Fact]
    public void Evaluar_DescripcionDeCalidad_NoMarcaNada()
    {
        var terminos = new[]
        {
            Termino("reunion", regla: ReglaTermino.GenericoSiVaSolo, severidad: SeveridadTermino.Advertir),
            Termino("soporte", regla: ReglaTermino.GenericoSiVaSolo, severidad: SeveridadTermino.Advertir)
        };

        var resultado = EvaluadorDescripcion.Evaluar(
            "CIMA WebClient - Ajuste en modulo de evidencias para solicitudes de cancelacion.",
            terminos, Parametros, nombreProyecto: "CIMA WebClient");

        resultado.Hallazgos.Should().BeEmpty();
    }

    // ── Bloquea ──────────────────────────────────────────────────────────────

    [Fact]
    public void Bloquea_ConHallazgoDeSeveridadAdvertir_EsFalse()
    {
        var terminos = new[] { Termino("varios", severidad: SeveridadTermino.Advertir) };

        var resultado = EvaluadorDescripcion.Evaluar("varios", terminos, Parametros);

        resultado.Hallazgos.Should().NotBeEmpty();
        resultado.Bloquea.Should().BeFalse();
    }

    [Fact]
    public void Bloquea_ConAlMenosUnHallazgoDeSeveridadBloquear_EsTrue()
    {
        var terminos = new[] { Termino("varios", severidad: SeveridadTermino.Bloquear) };

        var resultado = EvaluadorDescripcion.Evaluar("varios", terminos, Parametros);

        resultado.Bloquea.Should().BeTrue();
    }
}
