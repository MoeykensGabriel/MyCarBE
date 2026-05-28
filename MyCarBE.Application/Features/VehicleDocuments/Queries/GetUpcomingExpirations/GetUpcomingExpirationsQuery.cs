using MediatR;
using MyCarBE.Application.Features.VehicleDocuments.DTOs;

namespace MyCarBE.Application.Features.VehicleDocuments.Queries.GetUpcomingExpirations;

/// <summary>
/// Vencimientos próximos (incluyendo vencidos) de todos los vehículos del usuario actual,
/// dentro del horizonte de N días. Es la query que alimenta el badge global del cliente.
/// </summary>
public record GetUpcomingExpirationsQuery(int HorizonDays)
    : IRequest<IReadOnlyList<UpcomingExpirationDto>>;
