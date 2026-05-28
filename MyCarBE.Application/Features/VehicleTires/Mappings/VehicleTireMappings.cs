using Mapster;
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTires.Mappings;

public class VehicleTireMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // VehicleTireDto se construye explícitamente en los handlers porque
        // requiere la Estimation calculada (que depende del km actual del vehículo).
        // Acá solo registramos lo que SÍ mapea 1-1.
        config.NewConfig<VehicleTireMeasurement, VehicleTireMeasurementDto>();
    }
}
