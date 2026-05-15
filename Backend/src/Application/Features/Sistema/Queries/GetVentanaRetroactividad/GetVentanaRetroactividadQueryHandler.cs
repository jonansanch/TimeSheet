using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetVentanaRetroactividad;

public class GetVentanaRetroactividadQueryHandler : IRequestHandler<GetVentanaRetroactividadQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetVentanaRetroactividadQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(GetVentanaRetroactividadQuery request, CancellationToken cancellationToken)
    {
        var param = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.VentanaRetroactividad, cancellationToken);

        return param != null && int.TryParse(param.Valor, out var dias) ? dias : 3;
    }
}
