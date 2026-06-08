using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ScheduleService;

public class ScheduleServiceCommandHandler : IRequestHandler<ScheduleServiceCommand>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork          _unitOfWork;

    public ScheduleServiceCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork          unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task Handle(ScheduleServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _workOrderRepository.GetServiceByIdAsync(request.WorkOrderServiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId);

        // No agendamos servicios de órdenes ya cerradas
        if (service.WorkOrder.CurrentStatus is WorkOrderStatus.Completed
                                            or WorkOrderStatus.Delivered
                                            or WorkOrderStatus.Cancelled)
            throw new BadRequestException(
                $"No se puede agendar un servicio de una orden en estado '{service.WorkOrder.CurrentStatus}'.");

        // Caso 1: borrar la programación
        if (request.ScheduledStart is null && request.ScheduledEnd is null)
        {
            service.ScheduledStart = null;
            service.ScheduledEnd   = null;
        }
        else
        {
            if (request.ScheduledStart is null)
                throw new BadRequestException("Se requiere ScheduledStart cuando se agenda.");

            var end = request.ScheduledEnd;

            // Default: si no vino end y el mecánico estimó duración, calcular automáticamente.
            // EstimatedDurationMinutes está en minutos (1 día laboral = 480 min), así que el
            // End queda en Start + duración exacta (permite trabajos de horas, no solo días).
            if (end is null && service.EstimatedDurationMinutes is int mins && mins > 0)
                end = request.ScheduledStart.Value.AddMinutes(mins);

            if (end is not null && end < request.ScheduledStart)
                throw new BadRequestException("ScheduledEnd no puede ser anterior a ScheduledStart.");

            service.ScheduledStart = request.ScheduledStart;
            service.ScheduledEnd   = end;
        }

        _workOrderRepository.Update(service.WorkOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
