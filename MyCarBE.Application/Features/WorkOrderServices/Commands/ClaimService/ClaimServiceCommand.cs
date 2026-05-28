using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ClaimService;

/// <summary>
/// Un mecánico autenticado se auto-asigna un servicio del pool de trabajos disponibles.
/// El mechanicId NO se pasa por body — se resuelve del ICurrentUserService para evitar
/// que un mecánico se haga pasar por otro.
/// </summary>
public record ClaimServiceCommand(Guid WorkOrderServiceId) : IRequest;
