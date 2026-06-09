using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Schedule.DTOs;

/// <summary>Área (servicio) que necesita un vehículo agendado. AreaId/AreaName null = servicio sin área.</summary>
public record OccupancyAreaDto(
    Guid?   AreaId,
    string? AreaName
);

/// <summary>
/// Una orden (vehículo) ocupando una bahía física en el calendario de ocupación.
/// El FE arma el tablero por día → servicio/área intersectando [ScheduledStart, ScheduledEnd] con cada día,
/// y ubica el vehículo en cada una de las áreas de sus servicios (Areas).
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
    string?         OwnerName,
    // Áreas distintas de los servicios (no borrados, no rechazados) del vehículo.
    // El FE pone una fila por área y mete el chip del vehículo en cada una.
    IReadOnlyList<OccupancyAreaDto> Areas
);
