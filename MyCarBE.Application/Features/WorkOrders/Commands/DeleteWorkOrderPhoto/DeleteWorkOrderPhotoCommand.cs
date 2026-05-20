using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Commands.DeleteWorkOrderPhoto;

public record DeleteWorkOrderPhotoCommand(
    Guid WorkOrderId,
    Guid PhotoId
) : IRequest<WorkOrderDetailDto>;
