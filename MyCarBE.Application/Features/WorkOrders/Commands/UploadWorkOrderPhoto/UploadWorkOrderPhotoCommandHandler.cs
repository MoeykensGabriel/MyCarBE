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
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public UploadWorkOrderPhotoCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IFileStorageService  fileStorage,
        IUnitOfWork          unitOfWork,
        IMapper              mapper)
    {
        _workOrderRepository = workOrderRepository;
        _fileStorage         = fileStorage;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(UploadWorkOrderPhotoCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        // Find existing vehicle-level photo of the same type (WorkOrderServiceId == null)
        var existing = workOrder.Photos
            .FirstOrDefault(p => p.PhotoType == request.PhotoType && p.WorkOrderServiceId == null);

        // Save new file first
        var folder = $"work-orders/{workOrder.Id}";
        var url    = await _fileStorage.SaveAsync(request.FileStream, request.FileName, folder, cancellationToken);

        // Replace old photo: soft-delete record + remove physical file
        string? oldUrl = null;
        if (existing != null)
        {
            oldUrl = existing.Url;
            workOrder.Photos.Remove(existing);
        }

        workOrder.Photos.Add(new WorkOrderPhoto
        {
            WorkOrderId = workOrder.Id,
            PhotoType   = request.PhotoType,
            Url         = url,
            Caption     = request.Caption,
            TakenAt     = DateTime.UtcNow,
        });

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete old file only after DB commit succeeds
        if (oldUrl is not null)
            await _fileStorage.DeleteAsync(oldUrl, cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
