using MyCarBE.Application.Features.VehicleDocuments.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleDocumentRepository : IRepository<VehicleDocument>
{
    /// <summary>Documentos de un vehículo, ordenados por ExpiresOn ascendente (los más próximos primero).</summary>
    Task<IReadOnlyList<VehicleDocument>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Próximos vencimientos (e incluso vencidos) para los vehículos de un Customer particular.
    /// horizon = días hacia adelante a considerar.
    /// </summary>
    Task<IReadOnlyList<UpcomingExpirationDto>> GetUpcomingForCustomerAsync(
        Guid customerId, int horizonDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Próximos vencimientos para los vehículos de una flota.
    /// </summary>
    Task<IReadOnlyList<UpcomingExpirationDto>> GetUpcomingForFleetAsync(
        Guid fleetId, int horizonDays, CancellationToken cancellationToken = default);
}
