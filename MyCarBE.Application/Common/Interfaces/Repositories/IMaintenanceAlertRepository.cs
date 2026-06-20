using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IMaintenanceAlertRepository : IRepository<MaintenanceAlert>
{
    /// <summary>
    /// Alertas configuradas (no borradas) de un vehículo, trackeadas para poder
    /// actualizarlas/borrarlas en el comando de "set replace". Ordenadas por tipo.
    /// </summary>
    Task<IReadOnlyList<MaintenanceAlert>> GetByVehicleIdAsync(
        Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Alertas de todos los vehículos de un dueño (customer XOR fleet), con el vehículo
    /// cargado (para el resumen del customer). Si ambos ids son null, devuelve vacío.
    /// </summary>
    Task<IReadOnlyList<MaintenanceAlert>> GetActiveByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default);
}
