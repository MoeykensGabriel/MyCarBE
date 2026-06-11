using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleMileageReadingRepository : IRepository<VehicleMileageReading>
{
    /// <summary>
    /// Últimas lecturas del vehículo, más recientes primero. <paramref name="take"/>
    /// acota el histórico (la trazabilidad típica son las últimas semanas, no años).
    /// </summary>
    Task<IReadOnlyList<VehicleMileageReading>> GetLatestByVehicleAsync(
        Guid vehicleId, int take, CancellationToken cancellationToken = default);
}
