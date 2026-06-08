using MediatR;
using MyCarBE.Application.Features.Schedule.DTOs;

namespace MyCarBE.Application.Features.Schedule.Queries.GetOccupancy;

/// <summary>
/// Ocupación física del taller en [From, To]: órdenes agendadas (post-aprobación) que
/// ocupan bahía, intersectando el rango, más la capacidad configurable.
/// </summary>
public record GetOccupancyQuery(DateTime From, DateTime To)
    : IRequest<OccupancyDto>;
