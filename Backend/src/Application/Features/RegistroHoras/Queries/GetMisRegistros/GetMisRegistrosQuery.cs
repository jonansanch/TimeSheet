namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record GetMisRegistrosQuery(DateOnly? Desde, DateOnly? Hasta) : IRequest<IEnumerable<MisRegistrosItemDto>>;
