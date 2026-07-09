using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteServiceAsWorkshop;

/// <summary>
/// La oficina (admin/recepción) finaliza un trabajo en curso en nombre del taller — para
/// destrabar servicios cuyo mecánico no va a continuar (conflicto, renuncia, ausencia).
/// </summary>
public record CompleteServiceAsWorkshopCommand(
    Guid   WorkOrderServiceId,
    string Notes
) : IRequest;
