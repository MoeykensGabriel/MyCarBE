using MediatR;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public record CreateWorkOrderCommand(
    Guid    VehicleId,
    int     MileageAtEntry,
    string? CustomerNote,
    string? ContactPersonName  = null,
    string? ContactPersonPhone = null
) : IRequest<Guid>;
