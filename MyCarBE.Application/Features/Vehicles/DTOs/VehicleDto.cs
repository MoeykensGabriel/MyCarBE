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
    DateTime        CreatedAt,
    DateTime        UpdatedAt
);
