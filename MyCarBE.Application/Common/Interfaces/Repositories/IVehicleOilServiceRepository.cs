using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IVehicleOilServiceRepository : IRepository<VehicleOilService>
{
    /// <summary>
    /// Último cambio de aceite registrado del vehículo (el más reciente por fecha), o null
    /// si todavía no hay ninguno. Es el que define el estado / próximo service "actual".
    /// </summary>
    Task<VehicleOilService?> GetLatestByVehicleAsync(
        Guid vehicleId, CancellationToken cancellationToken = default);
}
