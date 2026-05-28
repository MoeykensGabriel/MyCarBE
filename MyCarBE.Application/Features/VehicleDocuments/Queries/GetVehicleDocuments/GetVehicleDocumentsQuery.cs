using MediatR;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;

namespace MyCarBE.Application.Features.VehicleDocuments.Queries.GetVehicleDocuments;

public record GetVehicleDocumentsQuery(Guid VehicleId)
    : IRequest<IReadOnlyList<VehicleDocumentDto>>;
