using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyTasks;

/// <summary>
/// Servicios asignados al mecánico actual (resuelto por JWT.mechanicId).
/// Si Status es null, devuelve activos (Pending + Accepted).
/// </summary>
public record GetMyTasksQuery(WorkOrderServiceAssignmentStatus? Status) : IRequest<IReadOnlyList<MechanicTaskDto>>;
