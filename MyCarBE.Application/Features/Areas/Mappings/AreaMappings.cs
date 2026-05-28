using Mapster;
using MyCarBE.Application.Features.Areas.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Areas.Mappings;

public class AreaMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Area, AreaDto>();
    }
}
