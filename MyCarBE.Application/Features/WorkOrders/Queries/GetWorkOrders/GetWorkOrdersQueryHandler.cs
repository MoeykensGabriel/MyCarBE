using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;

public class GetWorkOrdersQueryHandler : IRequestHandler<GetWorkOrdersQuery, PagedResult<WorkOrderSummaryDto>>
{
    private readonly IWorkOrderRepository _repository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IMapper              _mapper;

    public GetWorkOrdersQueryHandler(
        IWorkOrderRepository repository,
        ICurrentUserService  currentUser,
        IMapper              mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<PagedResult<WorkOrderSummaryDto>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var page      = Math.Max(1, request.Page);
        var pageSize  = Math.Clamp(request.PageSize, 1, 100);
        var status    = request.Status;
        var search    = request.Search;
        var ownerType = request.OwnerType;

        // Customer: ignora query params, usa sus propios IDs del JWT
        if (!_currentUser.IsAdmin)
        {
            if (_currentUser.FleetId.HasValue)
                return await MapPagedAsync(
                    await _repository.GetByFleetIdAtEntryPagedAsync(_currentUser.FleetId.Value, status, search, ownerType, page, pageSize, cancellationToken));

            if (_currentUser.CustomerId.HasValue)
                return await MapPagedAsync(
                    await _repository.GetByCustomerIdAtEntryPagedAsync(_currentUser.CustomerId.Value, status, search, ownerType, page, pageSize, cancellationToken));

            return new PagedResult<WorkOrderSummaryDto>([], 0, page, pageSize);
        }

        // Admin: filtra por el parámetro provisto; sin filtro → todas las órdenes
        var paged = request switch
        {
            { VehicleId:  { } id } => await _repository.GetByVehicleIdPagedAsync(id, status, search, ownerType, page, pageSize, cancellationToken),
            { CustomerId: { } id } => await _repository.GetByCustomerIdAtEntryPagedAsync(id, status, search, ownerType, page, pageSize, cancellationToken),
            { FleetId:    { } id } => await _repository.GetByFleetIdAtEntryPagedAsync(id, status, search, ownerType, page, pageSize, cancellationToken),
            _                      => await _repository.GetAllPagedAsync(status, search, ownerType, page, pageSize, cancellationToken)
        };

        return await MapPagedAsync(paged);
    }

    private Task<PagedResult<WorkOrderSummaryDto>> MapPagedAsync(PagedResult<Domain.Entities.WorkOrder> source)
    {
        var items = _mapper.Map<IReadOnlyList<WorkOrderSummaryDto>>(source.Items);
        return Task.FromResult(new PagedResult<WorkOrderSummaryDto>(items, source.TotalCount, source.Page, source.PageSize));
    }
}
