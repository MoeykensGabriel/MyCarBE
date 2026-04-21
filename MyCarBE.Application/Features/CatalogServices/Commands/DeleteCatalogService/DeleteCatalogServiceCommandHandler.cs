using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.CatalogServices.Commands.DeleteCatalogService;

public class DeleteCatalogServiceCommandHandler : IRequestHandler<DeleteCatalogServiceCommand>
{
    private readonly ICatalogServiceRepository _repository;
    private readonly IUnitOfWork               _unitOfWork;

    public DeleteCatalogServiceCommandHandler(
        ICatalogServiceRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCatalogServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogService), request.Id);

        _repository.Delete(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
