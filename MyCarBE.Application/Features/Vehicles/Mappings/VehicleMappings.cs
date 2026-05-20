using Mapster;
using MyCarBE.Application.Features.Vehicles.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Vehicles.Mappings;

public class VehicleMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Vehicle, VehicleDto>();
    }
}
