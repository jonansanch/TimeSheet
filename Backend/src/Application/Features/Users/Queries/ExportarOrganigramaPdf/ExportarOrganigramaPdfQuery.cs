using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Queries.ExportarOrganigramaPdf;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record ExportarOrganigramaPdfQuery : IRequest<ExportarOrganigramaPdfResult>;

public record ExportarOrganigramaPdfResult(byte[] Contenido, string ContentType, string FileName);
