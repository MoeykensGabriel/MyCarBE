using MapsterMapper;
using MyCarBE.Application.Features.VehicleBatteries.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleBatteries;

/// <summary>
/// Construye el DTO de batería con el estado actual derivado del último chequeo.
/// </summary>
public static class VehicleBatteryDtoFactory
{
    public static VehicleBatteryDto Build(VehicleBattery battery, IMapper mapper)
    {
        var checks = battery.Checks
            .OrderBy(c => c.CheckedOn)
            .Select(mapper.Map<VehicleBatteryCheckDto>)
            .ToList();

        var last = battery.Checks
            .OrderByDescending(c => c.CheckedOn)
            .FirstOrDefault();

        return new VehicleBatteryDto(
            Id:             battery.Id,
            VehicleId:      battery.VehicleId,
            Brand:          battery.Brand,
            ManufacturedOn: battery.ManufacturedOn,
            InstalledOn:    battery.InstalledOn,
            InstalledAtKm:  battery.InstalledAtKm,
            IsActive:       battery.IsActive,
            ReplacedOn:     battery.ReplacedOn,
            ReplacedAtKm:   battery.ReplacedAtKm,
            CreatedAt:      battery.CreatedAt,
            UpdatedAt:      battery.UpdatedAt,
            Checks:         checks,
            CurrentStatus:  last?.Status,
            LastCheckedOn:  last?.CheckedOn
        );
    }
}
