using Microsoft.AspNetCore.Identity;

namespace KPG.Timesheet.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeactivatedAt { get; set; }
    public string? DeactivatedBy { get; set; }

    /// <summary>
    /// Jefe directo. Define el organigrama y es la segunda aprobacion del timesheet.
    /// Nulo en la cima de la jerarquia.
    /// </summary>
    public string? SupervisorUserId { get; set; }

    /// <summary>
    /// Puesto de la persona, del catalogo Empleados. Alimenta el campo Recurso del
    /// registro y resuelve quien da la primera aprobacion.
    /// </summary>
    public int? PuestoId { get; set; }
}
