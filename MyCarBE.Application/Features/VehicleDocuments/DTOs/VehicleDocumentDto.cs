using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.DTOs;

public record VehicleDocumentDto(
    Guid                Id,
    Guid                VehicleId,
    VehicleDocumentType DocumentType,
    DateOnly            ExpiresOn,
    string?             Notes,
    string?             IssuingEntity,
    DateTime            CreatedAt,
    DateTime            UpdatedAt
);
