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

    /// <summary>
    /// DEPRECATED: especialidad como texto libre. La fuente de verdad ahora es Areas (M-a-N).
    /// Se conserva para no perder datos históricos; nuevas asignaciones usan Areas.
    /// </summary>
    public string? Specialty { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Mecánico generalista: puede reportar/trabajar en TODAS las áreas activas durante la
    /// inspección, sin que el admin se las asigne una por una. No se modela como área (eso
    /// la volvería obligatoria en cada cierre de inspección); es una capacidad del mecánico.
    /// </summary>
    public bool IsGeneralist { get; set; }

    // FK al ApplicationUser — solo el Guid, sin navegación (Domain no depende de Identity)
    public Guid ApplicationUserId { get; set; }

    // Navegación (queries de "mis servicios asignados")
    public ICollection<WorkOrderService> AssignedServices { get; set; } = new List<WorkOrderService>();

    // Áreas de especialidad — relación M-a-N con Area
    public ICollection<Area> Areas { get; set; } = new List<Area>();
}
