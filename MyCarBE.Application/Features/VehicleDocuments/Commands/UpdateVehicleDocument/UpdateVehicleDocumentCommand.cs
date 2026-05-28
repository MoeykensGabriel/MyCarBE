using MediatR;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.UpdateVehicleDocument;

public record UpdateVehicleDocumentCommand(
    Guid                Id,
    VehicleDocumentType DocumentType,
    DateOnly            ExpiresOn,
    string?             Notes,
    string?             IssuingEntity
) : IRequest<VehicleDocumentDto>;
