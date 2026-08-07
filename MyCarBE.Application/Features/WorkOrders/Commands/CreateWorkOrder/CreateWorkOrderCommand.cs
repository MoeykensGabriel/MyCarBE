using MediatR;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public record CreateWorkOrderCommand(
    Guid    VehicleId,
    int     MileageAtEntry,
    string? CustomerNote,
    string? ContactPersonName  = null,
    string? ContactPersonPhone = null,
    /// <summary>
    /// Motivo por el que el cliente trae el vehículo. Opcional a nivel command (compatibilidad
    /// hasta S3-14); el validador lo hará obligatorio cuando el wizard lo envíe.
    /// </summary>
    string? ServiceReason      = null,
    /// <summary>
    /// Para qué entra el vehículo. Default Repair: el ingreso que no lo mande explícitamente
    /// crea una orden de trabajo normal, como siempre.
    /// </summary>
    WorkOrderPurpose Purpose   = WorkOrderPurpose.Repair
) : IRequest<Guid>;
