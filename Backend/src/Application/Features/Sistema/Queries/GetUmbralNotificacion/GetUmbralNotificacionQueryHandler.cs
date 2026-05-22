using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetUmbralNotificacion;

public class GetUmbralNotificacionQueryHandler : IRequestHandler<GetUmbralNotificacionQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetUmbralNotificacionQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<int> Handle(GetUmbralNotificacionQuery request, CancellationToken cancellationToken)
    {
        var param = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.DiasUmbralNotificacion, cancellationToken);

        return param != null && int.TryParse(param.Valor, out var dias) ? dias : 3;
    }
}
