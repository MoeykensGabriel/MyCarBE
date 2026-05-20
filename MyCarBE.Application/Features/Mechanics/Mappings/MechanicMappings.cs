using Mapster;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Mappings;

public class MechanicMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Mechanic, MechanicDto>();
    }
}
