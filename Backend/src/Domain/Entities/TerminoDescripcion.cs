using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Termino o frase del catalogo de calidad de descripciones del timesheet (ver
/// Docs/plan-calidad-descripciones.md). Lo mantiene el Admin desde
/// <c>/api/terminos-descripcion</c> y lo evalua <c>IValidadorDescripcion</c> al guardar un
/// registro y en el aviso en vivo del formulario.
/// </summary>
public class TerminoDescripcion : BaseAuditableEntity
{
    private TerminoDescripcion() { }

    public TerminoDescripcion(
        string termino,
        TipoTermino tipo,
        ReglaTermino regla,
        SeveridadTermino severidad,
        string motivo,
        string? sugerencia = null)
    {
        ValidarTermino(termino);
        ValidarMotivo(motivo);
        ValidarSugerencia(sugerencia);

        Termino = termino.Trim();
        TerminoNormalizado = NormalizadorTexto.Normalizar(termino);
        Tipo = tipo;
        Regla = regla;
        Severidad = severidad;
        Motivo = motivo.Trim();
        Sugerencia = string.IsNullOrWhiteSpace(sugerencia) ? null : sugerencia.Trim();
        Activo = true;
    }

    /// <summary>Texto tal como lo escribio el administrador.</summary>
    public string Termino { get; private set; } = string.Empty;

    /// <summary>
    /// <see cref="Termino"/> pasado por <see cref="NormalizadorTexto.Normalizar"/>. Es lo que
    /// compara el evaluador contra la descripcion (tambien normalizada) y lleva el indice
    /// unico: no puede haber dos terminos que normalicen igual.
    /// </summary>
    public string TerminoNormalizado { get; private set; } = string.Empty;

    public TipoTermino Tipo { get; private set; }

    public ReglaTermino Regla { get; private set; }

    public SeveridadTermino Severidad { get; private set; }

    /// <summary>Reemplazo sugerido, con marcadores <c>[Asi]</c> para que el usuario complete.</summary>
    public string? Sugerencia { get; private set; }

    /// <summary>Por que el termino es una falla de calidad; se muestra en el aviso.</summary>
    public string Motivo { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public void Actualizar(
        string termino,
        TipoTermino tipo,
        ReglaTermino regla,
        SeveridadTermino severidad,
        string motivo,
        string? sugerencia)
    {
        ValidarTermino(termino);
        ValidarMotivo(motivo);
        ValidarSugerencia(sugerencia);

        Termino = termino.Trim();
        TerminoNormalizado = NormalizadorTexto.Normalizar(termino);
        Tipo = tipo;
        Regla = regla;
        Severidad = severidad;
        Motivo = motivo.Trim();
        Sugerencia = string.IsNullOrWhiteSpace(sugerencia) ? null : sugerencia.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static void ValidarTermino(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            throw new DomainRuleException("El termino es requerido.");
        if (termino.Trim().Length > 100)
            throw new DomainRuleException("El termino no puede superar 100 caracteres.");
    }

    private static void ValidarMotivo(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainRuleException("El motivo es requerido.");
        if (motivo.Trim().Length > 200)
            throw new DomainRuleException("El motivo no puede superar 200 caracteres.");
    }

    private static void ValidarSugerencia(string? sugerencia)
    {
        if (sugerencia is { Length: > 300 })
            throw new DomainRuleException("La sugerencia no puede superar 300 caracteres.");
    }
}
