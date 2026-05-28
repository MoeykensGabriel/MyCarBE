using MediatR;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.CreateVehicleDocument;

public record CreateVehicleDocumentCommand(
    Guid                VehicleId,
    VehicleDocumentType DocumentType,
    DateOnly            ExpiresOn,
    string?             Notes,
    string?             IssuingEntity
) : IRequest<VehicleDocumentDto>;
