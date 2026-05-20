using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Mecánico del taller. Se le asignan WorkOrderService individuales.
/// Tiene su propia cuenta de login (ApplicationUser) con rol "Mechanic".
/// </summary>
public class Mechanic : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty; // único
    public string? Phone     { get; set; }
    public string? Specialty { get; set; } // texto libre: "Motor", "Frenos", etc.

    public bool IsActive { get; set; } = true;

    // FK al ApplicationUser — solo el Guid, sin navegación (Domain no depende de Identity)
    public Guid ApplicationUserId { get; set; }

    // Navegación (queries de "mis servicios asignados")
    public ICollection<WorkOrderService> AssignedServices { get; set; } = new List<WorkOrderService>();
}
