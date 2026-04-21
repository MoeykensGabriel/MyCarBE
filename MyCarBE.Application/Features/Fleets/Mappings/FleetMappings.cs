using Mapster;
using MyCarBE.Application.Features.Fleets.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Fleets.Mappings;

public class FleetMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Flat DTO — used for list responses
        config.NewConfig<Fleet, FleetDto>();

        // Detail DTO — includes summarized contacts and vehicles
        config.NewConfig<Fleet, FleetDetailDto>()
            .Map(dest => dest.Contacts, src => src.Contacts.Select(c => new FleetContactSummary(
                c.Id, c.FirstName, c.LastName, c.Phone, c.Email)).ToList())
            .Map(dest => dest.Vehicles, src => src.Vehicles.Select(v => new FleetVehicleSummary(
                v.Id, v.LicensePlate, v.Brand, v.Model, v.Year)).ToList());
    }
}
