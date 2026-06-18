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

    /// <summary>
    /// Última lectura conocida del odómetro. Cache de la VehicleMileageReading
    /// más reciente — actualizar SIEMPRE vía una lectura nueva, nunca a mano.
    /// </summary>
    public int CurrentMileage { get; set; }

    /// <summary>
    /// Cuándo se registró la última lectura de km (de cualquier fuente).
    /// Null = nunca se registró una lectura → el recordatorio aplica de entrada.
    /// </summary>
    public DateTime? MileageUpdatedAt { get; set; }

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

    /// <summary>
    /// Token único usado para identificar al vehículo en la estación pública de viajes
    /// (QR pegado adentro del auto). Null = el vehículo no tiene estación habilitada todavía.
    /// El encargado lo genera / regenera desde su panel.
    /// </summary>
    public string? TripToken { get; set; }

    // Navegación
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<MaintenanceAlert> MaintenanceAlerts { get; set; } = new List<MaintenanceAlert>();
    public ICollection<DeclaredServiceHistory> DeclaredServiceHistories { get; set; } = new List<DeclaredServiceHistory>();
    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public ICollection<VehicleTrip> Trips { get; set; } = new List<VehicleTrip>();
    public ICollection<VehicleTire> Tires { get; set; } = new List<VehicleTire>();
    public ICollection<VehicleBattery> Batteries { get; set; } = new List<VehicleBattery>();
    public ICollection<VehicleOilService> OilServices { get; set; } = new List<VehicleOilService>();
    public ICollection<VehicleMileageReading> MileageReadings { get; set; } = new List<VehicleMileageReading>();

    /// <summary>
    /// Registra una lectura nueva del odómetro y actualiza el cache. La fecha del
    /// recordatorio se renueva siempre (alguien miró el odómetro); el km solo
    /// avanza, nunca retrocede — la validación de monotonía vive en el handler,
    /// esto es la última línea de defensa.
    /// </summary>
    public VehicleMileageReading AddMileageReading(
        int mileage, Domain.Enums.MileageReadingSource source, Guid? reportedByUserId, Guid? workOrderId = null)
    {
        var reading = new VehicleMileageReading
        {
            // El Id lo asigna AppDbContext.SaveChangesAsync (patrón de BaseEntity).
            // Setearlo a mano sobre una clave ValueGeneratedOnAdd hace que EF trate
            // la lectura como una fila existente y emita UPDATE (0 filas) en vez de
            // INSERT → DbUpdateConcurrencyException.
            VehicleId        = Id,
            Mileage          = mileage,
            Source           = source,
            ReportedByUserId = reportedByUserId,
            WorkOrderId      = workOrderId,
        };
        MileageReadings.Add(reading);

        if (mileage >= CurrentMileage)
            CurrentMileage = mileage;
        MileageUpdatedAt = DateTime.UtcNow;

        return reading;
    }

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
