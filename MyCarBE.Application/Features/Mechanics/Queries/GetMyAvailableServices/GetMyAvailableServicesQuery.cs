using MediatR;
using MyCarBE.Application.Features.Mechanics.DTOs;

namespace MyCarBE.Application.Features.Mechanics.Queries.GetMyAvailableServices;

/// <summary>
/// Pool de trabajos que el mecánico autenticado puede auto-tomar.
/// No requiere parámetros — filtra por tenant implícitamente a través del estado del taller.
/// </summary>
public record GetMyAvailableServicesQuery() : IRequest<IReadOnlyList<AvailableServiceDto>>;
