namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetResumenMensual;

/// <summary>
/// Resumen por dia del mes para pintar el calendario de registro: cuantos minutos
/// sumo cada dia y cual es el umbral vigente para considerarlo completo.
/// </summary>
public record GetResumenMensualQuery(int Mes, int Anio) : IRequest<ResumenMensualResponse>;

public record ResumenMensualResponse(
    int MinutosDiaCompleto,
    IReadOnlyList<DiaResumenDto> Dias);

public record DiaResumenDto(DateOnly Fecha, int TotalMinutos);
