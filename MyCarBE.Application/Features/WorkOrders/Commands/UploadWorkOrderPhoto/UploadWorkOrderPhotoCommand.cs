using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UploadWorkOrderPhoto;

public record UploadWorkOrderPhotoCommand(
    Guid      WorkOrderId,
    PhotoType PhotoType,
    Stream    FileStream,
    string    FileName,
    string?   Caption
) : IRequest<WorkOrderDetailDto>;
