using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KPG.Timesheet.Infrastructure.Data.Interceptors;

public class RegistroHorasImmutabilityInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> AllowedModifiedProperties =
    [
        nameof(RegistroHoras.Descripcion),
        nameof(BaseAuditableEntity.LastModified),
        nameof(BaseAuditableEntity.LastModifiedBy)
    ];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Validate(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Validate(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Validate(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<RegistroHoras>())
        {
            if (entry.State != EntityState.Modified)
                continue;

            var hasForbiddenChange = entry.Properties
                .Any(property => property.IsModified && !AllowedModifiedProperties.Contains(property.Metadata.Name));

            if (hasForbiddenChange)
            {
                throw new DomainRuleException(
                    "Los registros de horas guardados son inmutables. Solo se permite actualizar la descripcion.");
            }
        }
    }
}
