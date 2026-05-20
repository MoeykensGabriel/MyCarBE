using Mapster;
using MyCarBE.Application.Features.Receptionists.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Receptionists.Mappings;

public class ReceptionistMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Receptionist, ReceptionistDto>();
    }
}
