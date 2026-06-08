using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ScheduleWorkOrder;

public class ScheduleWorkOrderCommandHandler : IRequestHandler<ScheduleWorkOrderCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public ScheduleWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(ScheduleWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), request.WorkOrderId);

        // No se agendan órdenes ya cerradas.
        if (workOrder.CurrentStatus is WorkOrderStatus.Delivered or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se puede agendar una orden en estado '{workOrder.CurrentStatus}'.");

        // Caso 1: borrar el agendado.
        if (request.ScheduledStart is null && request.ScheduledEnd is null)
        {
            workOrder.ScheduledStart = null;
            workOrder.ScheduledEnd   = null;
        }
        else
        {
            if (request.ScheduledStart is null)
                throw new BadRequestException("Se requiere ScheduledStart cuando se agenda.");

            var end = request.ScheduledEnd;

            // Default: fin = inicio + duración total estimada de los servicios activos.
            // Prioriza la estimación del mecánico (EstimatedDurationMinutes); si no estimó,
            // cae al snapshot del catálogo (EstimatedDurationMinutesSnapshot).
            if (end is null)
            {
                var totalMinutes = workOrder.Services
                    .Where(s => !s.IsDeleted)
                    .Sum(s => (s.EstimatedDurationMinutes ?? s.EstimatedDurationMinutesSnapshot) * s.Quantity);

                end = request.ScheduledStart.Value.AddMinutes(totalMinutes);
            }

            if (end < request.ScheduledStart)
                throw new BadRequestException("ScheduledEnd no puede ser anterior a ScheduledStart.");

            workOrder.ScheduledStart = request.ScheduledStart;
            workOrder.ScheduledEnd   = end;
        }

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
