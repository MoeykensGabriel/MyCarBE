using Mapster;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleDocuments.Mappings;

public class VehicleDocumentMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VehicleDocument, VehicleDocumentDto>();
    }
}
