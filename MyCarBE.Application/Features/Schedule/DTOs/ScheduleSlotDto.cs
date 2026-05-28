namespace MyCarBE.Application.Features.Schedule.DTOs;

/// <summary>
/// Una ocupación en el calendario del taller: un servicio agendado con su mecánico,
/// área y vehículo. El FE arma la grilla (día × área) intersectando el rango con cada día.
/// </summary>
public record ScheduleSlotDto(
    Guid     WorkOrderServiceId,
    Guid     WorkOrderId,
    string   ServiceName,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int?     EstimatedDays,
    Guid?    AreaId,
    string?  AreaName,
    Guid?    MechanicId,
    string?  MechanicFullName,
    // Vehículo
    Guid     VehicleId,
    string   VehicleLicensePlate,
    string   VehicleBrand,
    string   VehicleModel
);
