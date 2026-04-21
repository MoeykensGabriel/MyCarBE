using Mapster;
using MyCarBE.Application.Features.Customers.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Customers.Mappings;

public class CustomerMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDto>();
    }
}
