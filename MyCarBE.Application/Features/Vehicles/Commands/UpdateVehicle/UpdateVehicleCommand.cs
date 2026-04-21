using MediatR;
using MyCarBE.Application.Features.Vehicles.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Vehicles.Commands.UpdateVehicle;

public record UpdateVehicleCommand(
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
    string          RegistrationHolderFirstName,
    string          RegistrationHolderLastName,
    DocumentType    RegistrationHolderDocumentType,
    string          RegistrationHolderDocumentNumber,
    string?         RegistrationCertificateNumber,
    Guid?           CustomerId,
    Guid?           FleetId
) : IRequest<VehicleDto>;
