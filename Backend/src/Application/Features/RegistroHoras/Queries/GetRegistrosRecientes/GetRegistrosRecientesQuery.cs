namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;

public record GetRegistrosRecientesQuery(int Top = 5) : IRequest<IEnumerable<RegistroRecienteDto>>;
