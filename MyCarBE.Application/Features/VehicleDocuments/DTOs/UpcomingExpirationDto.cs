using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.DTOs;

/// <summary>
/// Vencimiento próximo con el contexto del vehículo, para el badge global del cliente.
/// </summary>
public record UpcomingExpirationDto(
    Guid                Id,
    Guid                VehicleId,
    string              VehicleLicensePlate,
    string              VehicleBrand,
    string              VehicleModel,
    VehicleDocumentType DocumentType,
    DateOnly            ExpiresOn,
    int                 DaysUntilExpiration   // negativo si ya venció
);
