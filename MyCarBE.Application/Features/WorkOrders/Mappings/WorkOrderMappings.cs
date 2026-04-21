using Mapster;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Mappings;

public class WorkOrderMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WorkOrder, WorkOrderSummaryDto>();

        config.NewConfig<WorkOrderStatusChange, WorkOrderStatusChangeDto>();

        config.NewConfig<WorkOrderService, WorkOrderServiceDto>()
            .Map(dest => dest.Subtotal, src => src.PriceSnapshot * src.Quantity);

        config.NewConfig<WorkOrder, WorkOrderDetailDto>()
            .Map(dest => dest.Services,  src => src.Services)
            .Map(dest => dest.Timeline,  src => src.StatusChanges);
    }
}
