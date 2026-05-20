using MediatR;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;

/// <summary>
/// Exactly one of CustomerId / FleetId must be provided (XOR).
/// The domain entity's ValidateOwnership() is called before persisting.
/// </summary>
public record CreateVehicleCommand(
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
    string          RegistrationHolderFirstName,
    string          RegistrationHolderLastName,
    DocumentType    RegistrationHolderDocumentType,
    string          RegistrationHolderDocumentNumber,
    string?         RegistrationCertificateNumber,
    Guid?           CustomerId,
    Guid?           FleetId
) : IRequest<Guid>;
