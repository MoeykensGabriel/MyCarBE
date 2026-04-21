using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string LicensePlate { get; set; } = string.Empty;    // Única
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? VIN { get; set; }                            // Único
    public string? EngineNumber { get; set; }
    public FuelType FuelType { get; set; }
    public VehicleBodyType VehicleBodyType { get; set; }
    public VehicleUseType VehicleUseType { get; set; }
    public string? Color { get; set; }
    public int CurrentMileage { get; set; }

    // Datos del titular según la cédula verde — puede diferir del Customer del sistema
    public string RegistrationHolderFirstName { get; set; } = string.Empty;
    public string RegistrationHolderLastName { get; set; } = string.Empty;
    public DocumentType RegistrationHolderDocumentType { get; set; }
    public string RegistrationHolderDocumentNumber { get; set; } = string.Empty;
    public string? RegistrationCertificateNumber { get; set; }

    // XOR: exactamente uno de los dos debe estar seteado (enforced también por CHECK constraint en DB)
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? FleetId { get; set; }
    public Fleet? Fleet { get; set; }

    // Navegación
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<MaintenanceAlert> MaintenanceAlerts { get; set; } = new List<MaintenanceAlert>();
    public ICollection<DeclaredServiceHistory> DeclaredServiceHistories { get; set; } = new List<DeclaredServiceHistory>();

    // -------------------------------------------------------------------------
    // Regla de negocio: XOR de titularidad
    // -------------------------------------------------------------------------

    /// <summary>
    /// Valida que el vehículo tenga exactamente un dueño: Customer XOR Fleet.
    /// Llamar antes de persistir.
    /// </summary>
    public void ValidateOwnership()
    {
        bool hasCustomer = CustomerId.HasValue;
        bool hasFleet = FleetId.HasValue;

        if (hasCustomer && hasFleet)
            throw new InvalidOperationException(
                "A vehicle cannot belong to both a Customer and a Fleet.");

        if (!hasCustomer && !hasFleet)
            throw new InvalidOperationException(
                "A vehicle must belong to either a Customer or a Fleet.");
    }
}
