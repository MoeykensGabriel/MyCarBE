using Mapster;
using MyCarBE.Application.Features.VehicleBatteries.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleBatteries.Mappings;

public class VehicleBatteryMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // VehicleBatteryDto se construye en el factory (CurrentStatus depende del último check).
        // Acá registramos solo el check, que mapea 1-1.
        config.NewConfig<VehicleBatteryCheck, VehicleBatteryCheckDto>();
    }
}
