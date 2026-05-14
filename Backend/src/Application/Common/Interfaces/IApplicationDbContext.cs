using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<RegistroHoras> RegistrosHoras { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
