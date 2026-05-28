using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UploadWorkOrderPhoto;

public class UploadWorkOrderPhotoCommandHandler : IRequestHandler<UploadWorkOrderPhotoCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IFileStorageService  _fileStorage;
    private readonly ICurrentUserService  _currentUser;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public UploadWorkOrderPhotoCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IFileStorageService  fileStorage,
        ICurrentUserService  currentUser,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _fileStorage         = fileStorage;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(UploadWorkOrderPhotoCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // ── Reglas de autorización dependiendo del rol ────────────────────────
        //
        // Mecánicos: solo pueden subir fotos vinculadas a un servicio que les esté
        // asignado. Esto se valida acá (no solo con [Authorize] en el controller)
        // porque depende de la relación entre el mecánico y el servicio puntual.
        //
        // Admin / Receptionist: pueden subir fotos del vehículo (sin servicio) o
        // de un servicio puntual si necesitan reforzar la documentación.

        // Mutuamente excluyentes
        if (request.WorkOrderServiceId is not null && request.InspectionReportId is not null)
            throw new BadRequestException(
                "La foto puede vincularse a un servicio O a un reporte de inspección, no a ambos.");

        if (_currentUser.IsMechanic)
        {
            // Mecánico SIEMPRE debe vincular la foto a algo suyo: un servicio asignado a él
            // o un reporte de inspección que él haya creado.
            if (request.WorkOrderServiceId is null && request.InspectionReportId is null)
                throw new ForbiddenException(
                    "Los mecánicos deben subir fotos vinculadas a un servicio o reporte propio.");

            if (request.WorkOrderServiceId is not null)
            {
                var service = workOrder.Services.FirstOrDefault(s => s.Id == request.WorkOrderServiceId.Value)
                    ?? throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId.Value);

                if (service.AssignedMechanicId != _currentUser.MechanicId)
                    throw new ForbiddenException(
                        "Solo podés subir fotos a servicios asignados a vos.");
            }
            else // InspectionReportId
            {
                var report = workOrder.InspectionReports
                    .FirstOrDefault(r => r.Id == request.InspectionReportId!.Value)
                    ?? throw new NotFoundException(nameof(InspectionReport), request.InspectionReportId!.Value);

                if (report.MechanicId != _currentUser.MechanicId)
                    throw new ForbiddenException(
                        "Solo podés subir fotos a reportes de inspección que vos cargaste.");
            }
        }
        else
        {
            // Admin / Receptionist: si targetea un servicio o reporte, validar que pertenezcan a la orden.
            if (request.WorkOrderServiceId is not null
                && !workOrder.Services.Any(s => s.Id == request.WorkOrderServiceId.Value))
                throw new NotFoundException(nameof(WorkOrderService), request.WorkOrderServiceId.Value);

            if (request.InspectionReportId is not null
                && !workOrder.InspectionReports.Any(r => r.Id == request.InspectionReportId.Value))
                throw new NotFoundException(nameof(InspectionReport), request.InspectionReportId.Value);
        }

        // ── Subir el archivo al storage ───────────────────────────────────────
        //
        // Nota: NO reemplazamos fotos del mismo tipo. Antes la lógica era "si ya
        // hay una Before, reemplazarla" — pero ahora las 6 fotos del intake son
        // todas Before y se pisaban entre sí. La oficina/admin puede borrar fotos
        // manualmente desde la pantalla de detalle si se sube una equivocada.

        var folder = $"work-orders/{workOrder.Id}";
        var url    = await _fileStorage.SaveAsync(request.FileStream, request.FileName, folder, cancellationToken);

        workOrder.Photos.Add(new WorkOrderPhoto
        {
            WorkOrderId        = workOrder.Id,
            WorkOrderServiceId = request.WorkOrderServiceId,
            InspectionReportId = request.InspectionReportId,
            PhotoType          = request.PhotoType,
            Url                = url,
            Caption            = request.Caption,
            TakenAt            = DateTime.UtcNow,
        });

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
