using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleBatteries.DTOs;

public record VehicleBatteryCheckDto(
    Guid          Id,
    Guid          VehicleBatteryId,
    DateTime      CheckedOn,
    int           VehicleMileageAtCheck,
    BatteryStatus Status,
    decimal?      Voltage,
    int?          RemainingPercentage,
    string?       Notes,
    Guid?         CheckedByUserId,
    Guid?         WorkOrderId,
    DateTime      CreatedAt
);
