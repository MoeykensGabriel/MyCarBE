using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;

namespace MyCarBE.Application.Features.Fleets.Commands.DeleteFleet;

public class DeleteFleetCommandHandler : IRequestHandler<DeleteFleetCommand>
{
    private readonly IFleetRepository _fleetRepository;
    private readonly IUnitOfWork      _unitOfWork;

    public DeleteFleetCommandHandler(IFleetRepository fleetRepository, IUnitOfWork unitOfWork)
    {
        _fleetRepository = fleetRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task Handle(DeleteFleetCommand request, CancellationToken cancellationToken)
    {
        var fleet = await _fleetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Fleet), request.Id);

        _fleetRepository.Delete(fleet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
