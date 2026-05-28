using MapsterMapper;
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTires;

/// <summary>
/// Construye el VehicleTireDto combinando el mapeo de la entidad + la estimación calculada.
/// Se factoriza acá porque tanto el query como los commands devuelven el mismo DTO.
/// </summary>
internal static class VehicleTireDtoFactory
{
    public static VehicleTireDto Build(VehicleTire tire, IMapper mapper, int? currentVehicleMileage)
    {
        var measurements = (tire.Measurements ?? new List<VehicleTireMeasurement>())
            .OrderBy(m => m.MeasuredOn)
            .Select(m => mapper.Map<VehicleTireMeasurementDto>(m))
            .ToList();

        var estimation = TireWearCalculator.Calculate(tire, currentVehicleMileage);

        return new VehicleTireDto(
            Id:                  tire.Id,
            VehicleId:           tire.VehicleId,
            Position:            tire.Position,
            Brand:               tire.Brand,
            Model:               tire.Model,
            SizeSpec:            tire.SizeSpec,
            InstalledOn:         tire.InstalledOn,
            InstalledAtKm:       tire.InstalledAtKm,
            InitialTreadDepthMm: tire.InitialTreadDepthMm,
            ExpectedLifeKm:      tire.ExpectedLifeKm,
            IsActive:            tire.IsActive,
            ReplacedOn:          tire.ReplacedOn,
            ReplacedAtKm:        tire.ReplacedAtKm,
            CreatedAt:           tire.CreatedAt,
            UpdatedAt:           tire.UpdatedAt,
            Measurements:        measurements,
            Estimation:          estimation
        );
    }
}
