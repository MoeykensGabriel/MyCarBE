using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Mechanics.Commands.AssignAreasToMechanic;

public class AssignAreasToMechanicCommandHandler : IRequestHandler<AssignAreasToMechanicCommand, MechanicDto>
{
    private readonly IMechanicRepository _mechanicRepository;
    private readonly IAreaRepository     _areaRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public AssignAreasToMechanicCommandHandler(
        IMechanicRepository mechanicRepository,
        IAreaRepository     areaRepository,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _mechanicRepository = mechanicRepository;
        _areaRepository     = areaRepository;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
    }

    public async Task<MechanicDto> Handle(AssignAreasToMechanicCommand request, CancellationToken cancellationToken)
    {
        var mechanic = await _mechanicRepository.GetByIdWithAreasAsync(request.MechanicId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mechanic), request.MechanicId);

        // Validar que todos los AreaIds existan y estén activas
        var areas = await _areaRepository.GetByIdsAsync(request.AreaIds, cancellationToken);

        if (areas.Count != request.AreaIds.Count)
        {
            var foundIds   = areas.Select(a => a.Id).ToHashSet();
            var missingIds = request.AreaIds.Where(id => !foundIds.Contains(id)).ToList();
            throw new BadRequestException(
                $"Las siguientes áreas no existen: {string.Join(", ", missingIds)}");
        }

        var inactive = areas.Where(a => !a.IsActive).Select(a => a.Name).ToList();
        if (inactive.Count > 0)
            throw new BadRequestException(
                $"No se pueden asignar áreas inactivas: {string.Join(", ", inactive)}");

        // Sincronización: reemplazo completo del set
        mechanic.Areas.Clear();
        foreach (var area in areas)
            mechanic.Areas.Add(area);

        _mechanicRepository.Update(mechanic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MechanicDto>(mechanic);
    }
}
