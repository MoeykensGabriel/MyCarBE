using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Schedule.DTOs;

/// <summary>
/// Una orden (vehículo) ocupando una bahía física en el calendario de ocupación.
/// El FE arma la grilla día × vehículo intersectando [ScheduledStart, ScheduledEnd] con cada día.
/// Status permite marcar visualmente: InProgress = trabajo activo, Completed = esperando retiro,
/// Approved = agendado pero todavía no presente.
/// </summary>
public record OccupancySlotDto(
    Guid            WorkOrderId,
    DateTime        ScheduledStart,
    DateTime        ScheduledEnd,
    WorkOrderStatus Status,
    Guid            VehicleId,
    string          VehicleLicensePlate,
    string          VehicleBrand,
    string          VehicleModel,
    string?         OwnerName
);
