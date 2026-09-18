using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetResumenMensual;

public class GetResumenMensualQueryHandler(
    IApplicationDbContext context,
    IUser user,
    IParametrosSistemaService parametros)
    : IRequestHandler<GetResumenMensualQuery, ResumenMensualResponse>
{
    public async Task<ResumenMensualResponse> Handle(
        GetResumenMensualQuery request,
        CancellationToken cancellationToken)
    {
        var minutosDiaCompleto = await parametros.GetMinutosDiaCompletoAsync(cancellationToken);

        var userId = user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return new ResumenMensualResponse(minutosDiaCompleto, []);

        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hastaExclusivo = desde.AddMonths(1);

        // Un dia puede tener varios registros (uno por cliente/proyecto): se agrupan
        // para que el calendario evalue el total del dia, no el de un registro suelto.
        var dias = await context.RegistrosHoras
            .Where(r => r.UserId == userId
                     && r.FechaRegistro >= desde
                     && r.FechaRegistro < hastaExclusivo)
            .GroupBy(r => r.FechaRegistro)
            .Select(g => new DiaResumenDto(
                g.Key,
                g.Sum(r =>
                    (r.HoraEntrada1 != null && r.HoraSalida1 != null
                        ? (r.HoraSalida1!.Value.Hour * 60 + r.HoraSalida1.Value.Minute)
                          - (r.HoraEntrada1!.Value.Hour * 60 + r.HoraEntrada1.Value.Minute)
                        : 0)
                  + (r.HoraEntrada2 != null && r.HoraSalida2 != null
                        ? (r.HoraSalida2!.Value.Hour * 60 + r.HoraSalida2.Value.Minute)
                          - (r.HoraEntrada2!.Value.Hour * 60 + r.HoraEntrada2.Value.Minute)
                        : 0)
                  + (r.HoraEntrada3 != null && r.HoraSalida3 != null
                        ? (r.HoraSalida3!.Value.Hour * 60 + r.HoraSalida3.Value.Minute)
                          - (r.HoraEntrada3!.Value.Hour * 60 + r.HoraEntrada3.Value.Minute)
                        : 0)))
            )
            .ToListAsync(cancellationToken);

        return new ResumenMensualResponse(minutosDiaCompleto, dias);
    }
}
