using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, IReadOnlyList<WorkOrderSummaryDto>>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IMapper              _mapper;

    public GetWorkOrdersQueryHandler(IWorkOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<WorkOrderSummaryDto>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<WorkOrder> orders = request switch
        {
            { VehicleId:  { } id } => await _repository.GetByVehicleIdAsync(id, cancellationToken),
            { CustomerId: { } id } => await _repository.GetByCustomerIdAtEntryAsync(id, cancellationToken),
            { FleetId:    { } id } => await _repository.GetByFleetIdAtEntryAsync(id, cancellationToken),
            _                      => Array.Empty<WorkOrder>()
        };

        return _mapper.Map<IReadOnlyList<WorkOrderSummaryDto>>(orders);
    }
}
