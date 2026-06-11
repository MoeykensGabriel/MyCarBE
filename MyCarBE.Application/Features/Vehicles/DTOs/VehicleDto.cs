using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Vehicles.DTOs;

public record VehicleDto(
    Guid            Id,
    string          LicensePlate,
    string          Brand,
    string          Model,
    int             Year,
    string?         VIN,
    string?         EngineNumber,
    FuelType        FuelType,
    VehicleBodyType VehicleBodyType,
    VehicleUseType  VehicleUseType,
    string?         Color,
    int             CurrentMileage,
    // Cédula verde
    string          RegistrationHolderFirstName,
    string          RegistrationHolderLastName,
    DocumentType    RegistrationHolderDocumentType,
    string          RegistrationHolderDocumentNumber,
    string?         RegistrationCertificateNumber,
    // Ownership
    Guid?           CustomerId,
    Guid?           FleetId,
    /// <summary>
    /// Token público de la estación de viajes (QR del chofer). Solo poblado para vehículos
    /// de flota. Para clientes particulares siempre null.
    /// </summary>
    string?         TripToken,
    DateTime        CreatedAt,
    DateTime        UpdatedAt
)
{
    /// <summary>Cuándo se registró la última lectura de km. Null = nunca.</summary>
    public DateTime? MileageUpdatedAt { get; init; }

    /// <summary>Días desde la última lectura de km. Null = nunca hubo una.</summary>
    public int? DaysSinceMileageUpdate { get; init; }

    /// <summary>
    /// True si el vehículo necesita que el cliente actualice el kilometraje
    /// (pasaron más días que el umbral del taller, o nunca se registró una lectura).
    /// Lo calcula el handler contra WorkshopSettings.MileageReminderDays.
    /// </summary>
    public bool MileageUpdateDue { get; init; }
}
