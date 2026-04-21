using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;  // Único
    public string Phone { get; set; } = string.Empty;           // Único
    public string? Email { get; set; }

    // FK al ApplicationUser — solo el Guid, sin navegación (Domain no depende de Identity)
    public Guid? ApplicationUserId { get; set; }

    // Si es contacto de una flota
    public Guid? FleetId { get; set; }
    public Fleet? Fleet { get; set; }

    // Navegación
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
