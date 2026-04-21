using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

public class Fleet : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;       // CUIT — único
    public string? Address { get; set; }

    // Navegación
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Customer> Contacts { get; set; } = new List<Customer>();  // Customers con FleetId = this.Id
}
