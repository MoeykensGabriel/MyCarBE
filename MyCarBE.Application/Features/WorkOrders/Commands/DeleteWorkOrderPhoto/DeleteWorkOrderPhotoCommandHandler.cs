using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Commands.DeleteWorkOrderPhoto;

public class DeleteWorkOrderPhotoCommandHandler : IRequestHandler<DeleteWorkOrderPhotoCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IFileStorageService  _fileStorage;
    private readonly IUnitOfWork          _unitOfWork;
    private readonly IMapper              _mapper;

    public DeleteWorkOrderPhotoCommandHandler(
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

    public async Task<WorkOrderDetailDto> Handle(DeleteWorkOrderPhotoCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        var photo = workOrder.Photos.FirstOrDefault(p => p.Id == request.PhotoId)
            ?? throw new NotFoundException(nameof(WorkOrderPhoto), request.PhotoId);

        var url = photo.Url;
        workOrder.Photos.Remove(photo);

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fileStorage.DeleteAsync(url, cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
