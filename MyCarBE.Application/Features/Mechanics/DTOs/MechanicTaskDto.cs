using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Mechanics.DTOs;

/// <summary>
/// Vista resumida de un servicio asignado al mecánico.
/// NO expone precios ni datos del cliente — solo lo necesario para que el mecánico
/// trabaje sobre el vehículo.
/// </summary>
public record MechanicTaskDto(
    Guid     WorkOrderServiceId,
    Guid     WorkOrderId,
    Guid     VehicleId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,

    /// <summary>
    /// Estado actual de la WorkOrder padre. El mecánico solo puede aceptar/completar
    /// cuando está en InProgress — exponerlo permite que el front deshabilite el botón
    /// en otros estados sin necesidad de un round-trip al back.
    /// </summary>
    WorkOrderStatus WorkOrderCurrentStatus,

    string   ServiceName,
    string   ServiceDescription,
    int      Quantity,

    WorkOrderServiceAssignmentStatus AssignmentStatus,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,

    string?  CustomerNote,    // lo que el cliente pidió originalmente
    string?  TechnicianNote,  // lo que el técnico anotó en el diagnóstico
    string?  MechanicNotes,
    string?  MechanicFindings,

    DateTime UpdatedAt
);
