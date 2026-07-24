using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.InspectionReports.Commands.ReopenInspectionArea;

public class ReopenInspectionAreaCommandHandler : IRequestHandler<ReopenInspectionAreaCommand, Unit>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IWorkOrderRepository        _workOrderRepository;
    private readonly IUnitOfWork                 _unitOfWork;

    public ReopenInspectionAreaCommandHandler(
        IInspectionReportRepository repository,
        IWorkOrderRepository        workOrderRepository,
        IUnitOfWork                 unitOfWork)
    {
        _repository          = repository;
        _workOrderRepository = workOrderRepository;
        _unitOfWork          = unitOfWork;
    }

    public async Task<Unit> Handle(ReopenInspectionAreaCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Después de cerrar la inspección los reportes quedan congelados; no se deshace.
        if (workOrder.CurrentStatus != WorkOrderStatus.UnderInspection)
            throw new BadRequestException(
                $"Solo se puede deshacer un área mientras la orden está en inspección " +
                $"(estado actual: {workOrder.CurrentStatus}).");

        var report = await _repository.GetByWorkOrderAndAreaAsync(
            request.WorkOrderId, request.AreaId, cancellationToken)
            ?? throw new NotFoundException(nameof(InspectionReport), $"{request.WorkOrderId}/{request.AreaId}");

        // Solo se deshacen las marcas de un click de la oficina. Un reporte de mecánico
        // (o de oficina con hallazgos) se edita por su flujo propio, no se borra acá.
        if (!report.IsSkipped && !report.IsNoFindings)
            throw new BadRequestException(
                "Solo se puede deshacer un área marcada 'sin novedades' o 'postergada' por la oficina.");

        _repository.Delete(report); // soft-delete → el área vuelve a quedar pendiente
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
