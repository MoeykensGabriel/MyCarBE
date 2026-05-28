using MediatR;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.ReleaseService;

/// <summary>
/// Un mecánico libera un servicio que se había tomado (estaba en Pending)
/// y todavía no aceptó formalmente. Vuelve al pool de trabajos disponibles.
/// </summary>
public record ReleaseServiceCommand(Guid WorkOrderServiceId) : IRequest;
