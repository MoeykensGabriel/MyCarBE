using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.CatalogServices.Commands.CreateCatalogService;

public class CreateCatalogServiceCommandHandler : IRequestHandler<CreateCatalogServiceCommand, Guid>
{
    private readonly ICatalogServiceRepository _repository;
    private readonly IUnitOfWork               _unitOfWork;

    public CreateCatalogServiceCommandHandler(
        ICatalogServiceRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCatalogServiceCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _repository.NameExistsAsync(request.Name, cancellationToken);
        if (nameExists)
            throw new ConflictException(nameof(CatalogService), nameof(request.Name), request.Name);

        var service = new CatalogService
        {
            Id                       = Guid.NewGuid(),
            Name                     = request.Name.Trim(),
            Description              = request.Description.Trim(),
            DefaultPrice             = request.DefaultPrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            IsActive                 = true
        };

        await _repository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return service.Id;
    }
}
