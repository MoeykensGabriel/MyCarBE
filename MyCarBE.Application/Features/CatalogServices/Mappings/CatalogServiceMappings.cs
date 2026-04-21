using Mapster;
using MyCarBE.Application.Features.CatalogServices.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.CatalogServices.Mappings;

public class CatalogServiceMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CatalogService, CatalogServiceDto>();
    }
}
