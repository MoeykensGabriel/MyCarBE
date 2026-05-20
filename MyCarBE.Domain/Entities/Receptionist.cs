using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Recepcionista del taller. Crea órdenes de trabajo reutilizando el wizard de intake.
/// Tiene su propia cuenta de login (ApplicationUser) con rol "Receptionist".
/// No se le asignan trabajos ni puede ver dashboards/órdenes ajenas.
/// </summary>
public class Receptionist : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty; // único

    public bool IsActive { get; set; } = true;

    // FK al ApplicationUser — solo el Guid, sin navegación (Domain no depende de Identity)
    public Guid ApplicationUserId { get; set; }
}
