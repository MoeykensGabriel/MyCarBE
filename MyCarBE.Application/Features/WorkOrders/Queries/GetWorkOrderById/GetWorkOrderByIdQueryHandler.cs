using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Security;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _repository;
    private readonly ICurrentUserService  _currentUser;
    private readonly IMapper              _mapper;

    public GetWorkOrderByIdQueryHandler(
        IWorkOrderRepository repository,
        ICurrentUserService  currentUser,
        IMapper              mapper)
    {
        _repository  = repository;
        _currentUser = currentUser;
        _mapper      = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _repository.GetWithFullDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.Id);

        WorkOrderAccess.EnsureCanView(workOrder, _currentUser);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
