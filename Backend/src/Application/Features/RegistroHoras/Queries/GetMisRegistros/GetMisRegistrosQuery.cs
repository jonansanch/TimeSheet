namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record GetMisRegistrosQuery() : IRequest<IEnumerable<MisRegistrosItemDto>>;
