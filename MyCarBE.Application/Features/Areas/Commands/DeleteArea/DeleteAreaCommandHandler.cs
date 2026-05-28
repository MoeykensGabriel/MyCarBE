using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Areas.Commands.DeleteArea;

/// <summary>
/// Soft-delete del área + IsActive=false. Las relaciones M-a-N con mecánicos
/// se conservan en la tabla puente, pero como el área queda IsDeleted=true,
/// el global query filter de BaseEntity la oculta de los listados.
/// Los reportes históricos (InspectionReport) que apuntan a esta área siguen
/// siendo válidos — solo se evita asignarla a nuevas órdenes/mecánicos.
/// </summary>
public class DeleteAreaCommandHandler : IRequestHandler<DeleteAreaCommand>
{
    private readonly IAreaRepository _repository;
    private readonly IUnitOfWork     _unitOfWork;

    public DeleteAreaCommandHandler(IAreaRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Area), request.Id);

        area.IsActive = false;
        _repository.Delete(area); // soft delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
