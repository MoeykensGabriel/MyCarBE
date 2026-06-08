using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleBatteries.DTOs;

public record VehicleBatteryDto(
    Guid       Id,
    Guid       VehicleId,
    string?    Brand,
    int?                 CapacityAh,
    decimal?             BoxWidthCm,
    decimal?             BoxLengthCm,
    decimal?             BoxHeightCm,
    BatteryTerminalSide? PositiveTerminalSide,
    DateOnly   InstalledOn,
    int        InstalledAtKm,
    bool       IsActive,
    DateOnly?  ReplacedOn,
    int?       ReplacedAtKm,
    DateTime   CreatedAt,
    DateTime   UpdatedAt,
    IReadOnlyList<VehicleBatteryCheckDto> Checks,
    // Estado actual = el del último chequeo (o null si todavía no hay ninguno).
    BatteryStatus? CurrentStatus,
    int?           CurrentRemainingPercentage,
    DateTime?      LastCheckedOn
);
