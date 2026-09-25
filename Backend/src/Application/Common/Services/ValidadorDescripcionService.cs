using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Application.Common.Services;

public class ValidadorDescripcionService(
    IApplicationDbContext context,
    IParametrosSistemaService parametrosSistema) : IValidadorDescripcion
{
    public async Task<EvaluacionDescripcion> EvaluarAsync(
        string? texto, int? proyectoId, CancellationToken cancellationToken = default)
    {
        var parametros = await CargarParametrosAsync(cancellationToken);
        if (!parametros.ValidacionActiva)
            return EvaluacionDescripcion.Vacia;

        var terminos = await CargarTerminosActivosAsync(cancellationToken);
        var nombreProyecto = await ResolverNombreProyectoAsync(proyectoId, cancellationToken);

        return EvaluadorDescripcion.Evaluar(texto, terminos, parametros, nombreProyecto);
    }

    public async Task<IReadOnlyList<EvaluacionDescripcion>> EvaluarVariasAsync(
        IReadOnlyList<(string? Texto, int? ProyectoId)> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
            return [];

        var parametros = await CargarParametrosAsync(cancellationToken);
        if (!parametros.ValidacionActiva)
            return items.Select(_ => EvaluacionDescripcion.Vacia).ToList();

        var terminos = await CargarTerminosActivosAsync(cancellationToken);

        // Una sola consulta para los nombres de todos los proyectos distintos que aparecen
        // en el lote, en vez de una por fila.
        var idsProyecto = items
            .Where(i => i.ProyectoId is > 0)
            .Select(i => i.ProyectoId!.Value)
            .Distinct()
            .ToList();
        var nombresPorId = idsProyecto.Count == 0
            ? new Dictionary<int, string>()
            : await context.Proyectos
                .Where(p => idsProyecto.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Nombre, cancellationToken);

        return items
            .Select(item => EvaluadorDescripcion.Evaluar(
                item.Texto, terminos, parametros,
                item.ProyectoId is > 0 && nombresPorId.TryGetValue(item.ProyectoId.Value, out var nombre) ? nombre : null))
            .ToList();
    }

    public async Task<IReadOnlyList<EvaluacionDescripcion>> EvaluarVariasPorNombreProyectoAsync(
        IReadOnlyList<(string? Texto, string? NombreProyecto)> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
            return [];

        var parametros = await CargarParametrosAsync(cancellationToken);
        if (!parametros.ValidacionActiva)
            return items.Select(_ => EvaluacionDescripcion.Vacia).ToList();

        var terminos = await CargarTerminosActivosAsync(cancellationToken);

        return items
            .Select(item => EvaluadorDescripcion.Evaluar(item.Texto, terminos, parametros, item.NombreProyecto))
            .ToList();
    }

    private Task<List<TerminoDescripcion>> CargarTerminosActivosAsync(CancellationToken cancellationToken) =>
        context.TerminosDescripcion.Where(t => t.Activo).ToListAsync(cancellationToken);

    private async Task<string?> ResolverNombreProyectoAsync(int? proyectoId, CancellationToken cancellationToken)
    {
        if (proyectoId is not > 0)
            return null;

        return await context.Proyectos
            .Where(p => p.Id == proyectoId)
            .Select(p => p.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ParametrosEvaluacionDescripcion> CargarParametrosAsync(CancellationToken cancellationToken)
    {
        var defecto = ParametrosEvaluacionDescripcion.PorDefecto;

        var activoTexto = await parametrosSistema.GetTextoAsync(
            ParametrosSistemaKeys.DescripcionValidacionActiva, defecto.ValidacionActiva.ToString(), cancellationToken);
        var minPalabras = await parametrosSistema.GetIntAsync(
            ParametrosSistemaKeys.DescripcionMinPalabras, defecto.MinPalabras, cancellationToken);
        var minContexto = await parametrosSistema.GetIntAsync(
            ParametrosSistemaKeys.DescripcionMinPalabrasContexto, defecto.MinPalabrasContexto, cancellationToken);
        var severidadTexto = await parametrosSistema.GetTextoAsync(
            ParametrosSistemaKeys.DescripcionSeveridadReglasBase, defecto.SeveridadReglasBase.ToString(), cancellationToken);

        return new ParametrosEvaluacionDescripcion(
            ValidacionActiva: bool.TryParse(activoTexto, out var activo) ? activo : defecto.ValidacionActiva,
            MinPalabras: minPalabras > 0 ? minPalabras : defecto.MinPalabras,
            MinPalabrasContexto: minContexto >= 0 ? minContexto : defecto.MinPalabrasContexto,
            SeveridadReglasBase: Enum.TryParse<SeveridadTermino>(severidadTexto, ignoreCase: true, out var severidad)
                ? severidad
                : defecto.SeveridadReglasBase);
    }
}
