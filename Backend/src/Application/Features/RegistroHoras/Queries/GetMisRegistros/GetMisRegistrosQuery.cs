namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record GetMisRegistrosQuery(DateOnly? Desde, DateOnly? Hasta, int Page = 1, int PageSize = 20)
    : IRequest<MisRegistrosPaginadosResponse>;

public record MisRegistrosPaginadosResponse(int TotalCount, IReadOnlyList<MisRegistrosItemDto> Items);
