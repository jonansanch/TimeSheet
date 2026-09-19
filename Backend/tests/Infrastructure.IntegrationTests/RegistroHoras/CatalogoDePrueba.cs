using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Desde la migracion a ProyectoId, los registros apuntan al catalogo y las queries
/// resuelven cliente y proyecto por join. Sin catalogo sembrado esas queries no
/// devuelven filas, asi que los tests que crean registros necesitan este seed.
/// </summary>
internal static class CatalogoDePrueba
{
    /// <summary>
    /// Crea <paramref name="proyectos"/> clientes con un proyecto cada uno, de forma que
    /// el proyecto N-esimo tenga Id == N y sea facil referenciarlo desde el test.
    /// </summary>
    public static async Task SembrarAsync(ApplicationDbContext context, int proyectos = 5)
    {
        if (context.Proyectos.Any()) return;

        for (var i = 1; i <= proyectos; i++)
        {
            var cliente = new Cliente($"Cliente {i}");
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync(CancellationToken.None);

            context.Proyectos.Add(new Proyecto(cliente.Id, $"Proyecto {i}"));
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
