using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Commands.AssignAreasToMechanic;

/// <summary>
/// Sincroniza el set de áreas de un mecánico con la lista provista.
/// PUT semantics: reemplaza completamente — pasar lista vacía deja al mecánico sin áreas.
/// </summary>
public record AssignAreasToMechanicCommand(
    Guid             MechanicId,
    IReadOnlyList<Guid> AreaIds
) : IRequest<MechanicDto>;
