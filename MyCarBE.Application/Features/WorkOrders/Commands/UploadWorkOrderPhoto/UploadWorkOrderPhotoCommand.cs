using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UploadWorkOrderPhoto;

public record UploadWorkOrderPhotoCommand(
    Guid      WorkOrderId,
    PhotoType PhotoType,
    Stream    FileStream,
    string    FileName,
    string?   Caption,
    /// <summary>
    /// Si está presente, la foto queda vinculada a ese servicio de la orden.
    /// Usado por mecánicos para subir fotos del trabajo que hicieron.
    /// Null = foto general de la orden (Before/After del vehículo entero).
    /// </summary>
    Guid?     WorkOrderServiceId = null,
    /// <summary>
    /// Si está presente, la foto queda vinculada a ese reporte de inspección.
    /// Usado por mecánicos al cargar su informe inicial por área.
    /// Mutuamente excluyente con WorkOrderServiceId (validado en el handler).
    /// </summary>
    Guid?     InspectionReportId = null
) : IRequest<WorkOrderDetailDto>;
