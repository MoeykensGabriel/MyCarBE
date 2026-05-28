using MediatR;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.DeleteVehicleDocument;

public record DeleteVehicleDocumentCommand(Guid Id) : IRequest;
