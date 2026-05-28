using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CloseInspection;

public class CloseInspectionCommandHandler : IRequestHandler<CloseInspectionCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IAreaRepository             _areaRepository;
    private readonly IInspectionReportRepository _inspectionReportRepository;
    private readonly ICurrentUserService         _currentUser;
    private readonly IUnitOfWork                 _unitOfWork;
    private readonly IMapper                     _mapper;

    public CloseInspectionCommandHandler(
        IWorkOrderRepository        workOrderRepository,
        IAreaRepository             areaRepository,
        IInspectionReportRepository inspectionReportRepository,
        ICurrentUserService         currentUser,
        IUnitOfWork                 unitOfWork,
        IMapper                     mapper)
    {
        _workOrderRepository        = workOrderRepository;
        _areaRepository             = areaRepository;
        _inspectionReportRepository = inspectionReportRepository;
        _currentUser                = currentUser;
        _unitOfWork                 = unitOfWork;
        _mapper                     = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(CloseInspectionCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        if (workOrder.CurrentStatus != WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede cerrar la inspección desde el estado UnderInspection " +
                $"(estado actual: {workOrder.CurrentStatus}).");

        // Verificar que todas las áreas activas estén cubiertas
        var activeAreas = await _areaRepository.GetAllAsync(includeInactive: false, cancellationToken);
        var reports     = await _inspectionReportRepository.GetByWorkOrderAsync(workOrder.Id, cancellationToken);
        var coveredAreaIds = reports.Select(r => r.AreaId).ToHashSet();

        var uncovered = activeAreas.Where(a => !coveredAreaIds.Contains(a.Id)).ToList();
        if (uncovered.Count > 0)
        {
            var names = string.Join(", ", uncovered.Select(a => a.Name));
            throw new BadRequestException(
                $"No se puede cerrar la inspección: faltan reportes en las siguientes áreas: {names}. " +
                "Pedí a los mecánicos correspondientes que reporten o marcalas como 'sin hallazgos'.");
        }

        workOrder.ChangeStatus(WorkOrderStatus.Diagnosing, _currentUser.UserId, "Inspección cerrada — pasa a cotización.");

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload con full details para el DTO
        var full = await _workOrderRepository.GetWithFullDetailsAsync(workOrder.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), workOrder.Id);
        return _mapper.Map<WorkOrderDetailDto>(full);
    }
}
