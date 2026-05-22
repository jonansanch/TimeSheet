using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KPG.Timesheet.Infrastructure.Data.Interceptors;

public class RegistroHorasImmutabilityInterceptor : SaveChangesInterceptor
{
    // Siempre modificables (metadata compartida del día)
    private static readonly HashSet<string> SiemprePermitidos =
    [
        nameof(RegistroHoras.Descripcion),
        nameof(RegistroHoras.Cliente),
        nameof(RegistroHoras.Proyecto),
        nameof(RegistroHoras.Modalidad),
        nameof(RegistroHoras.Recurso),
        nameof(RegistroHoras.Lugar),
        nameof(BaseAuditableEntity.LastModified),
        nameof(BaseAuditableEntity.LastModifiedBy)
    ];

    // Permitidos solo si el valor ANTERIOR era null (agregar un turno que no existía)
    private static readonly HashSet<string> SoloAgregables =
    [
        nameof(RegistroHoras.HoraEntradaAM),
        nameof(RegistroHoras.HoraSalidaAM),
        nameof(RegistroHoras.HoraEntradaPM),
        nameof(RegistroHoras.HoraSalidaPM)
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
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<RegistroHoras>())
        {
            if (entry.State != EntityState.Modified) continue;

            foreach (var property in entry.Properties.Where(p => p.IsModified))
            {
                var name = property.Metadata.Name;

                if (SiemprePermitidos.Contains(name)) continue;

                if (SoloAgregables.Contains(name))
                {
                    // Permitido si el valor original era null (se está agregando, no modificando)
                    var valorOriginal = entry.OriginalValues[name];
                    if (valorOriginal is null) continue;
                }

                throw new DomainRuleException(
                    "Los registros de horas guardados son inmutables. " +
                    "Solo se permite agregar un turno faltante o actualizar la descripcion y metadatos.");
            }
        }
    }
}
