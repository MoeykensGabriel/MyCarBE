using Mapster;
using MyCarBE.Application.Features.VehicleTrips.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTrips.Mappings;

public class VehicleTripMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VehicleTrip, VehicleTripDto>()
            .Map(d => d.VehicleLicensePlate, s => s.Vehicle.LicensePlate)
            .Map(d => d.VehicleBrand,        s => s.Vehicle.Brand)
            .Map(d => d.VehicleModel,        s => s.Vehicle.Model);
    }
}
