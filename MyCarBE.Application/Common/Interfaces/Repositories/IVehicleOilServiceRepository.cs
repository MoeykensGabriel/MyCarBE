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

    /// <summary>
    /// Último cambio de aceite de cada vehículo de un dueño (customer XOR fleet), con el
    /// vehículo cargado (para el km actual). Una sola query para el resumen de mantenimiento.
    /// Si ambos ids son null, devuelve vacío.
    /// </summary>
    Task<IReadOnlyList<VehicleOilService>> GetLatestByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default);
}
