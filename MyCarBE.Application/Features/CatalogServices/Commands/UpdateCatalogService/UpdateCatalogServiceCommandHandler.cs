using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.CatalogServices.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.CatalogServices.Commands.UpdateCatalogService;

public class UpdateCatalogServiceCommandHandler : IRequestHandler<UpdateCatalogServiceCommand, CatalogServiceDto>
{
    private readonly ICatalogServiceRepository _repository;
    private readonly IUnitOfWork               _unitOfWork;
    private readonly IMapper                   _mapper;

    public UpdateCatalogServiceCommandHandler(
        ICatalogServiceRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<CatalogServiceDto> Handle(UpdateCatalogServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogService), request.Id);

        var nameConflict = await _repository.NameExistsAsync(request.Name, request.Id, cancellationToken);
        if (nameConflict)
            throw new ConflictException(nameof(CatalogService), nameof(request.Name), request.Name);

        service.Name                     = request.Name.Trim();
        service.Description              = request.Description.Trim();
        service.DefaultPrice             = request.DefaultPrice;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        service.IsActive                 = request.IsActive;

        _repository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CatalogServiceDto>(service);
    }
}
