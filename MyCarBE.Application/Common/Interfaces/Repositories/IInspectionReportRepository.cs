using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IInspectionReportRepository : IRepository<InspectionReport>
{
    /// <summary>Reportes de una orden con Area y Mechanic eager-loaded (para panel admin).</summary>
    Task<IReadOnlyList<InspectionReport>> GetByWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken = default);

    /// <summary>Devuelve el reporte de un área específica de una orden, o null si no existe.</summary>
    Task<InspectionReport?> GetByWorkOrderAndAreaAsync(Guid workOrderId, Guid areaId, CancellationToken cancellationToken = default);

    /// <summary>True si ya existe un reporte para esa (workOrderId, areaId).</summary>
    Task<bool> ExistsForAreaAsync(Guid workOrderId, Guid areaId, CancellationToken cancellationToken = default);

    /// <summary>Reporte por Id con ProposedServices/ProposedParts cargados (para edición y conversión).</summary>
    Task<InspectionReport?> GetByIdWithProposalsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Marca como soft-deleted todas las propuestas del reporte (servicios y repuestos).</summary>
    void RemoveAllProposals(InspectionReport report);

    /// <summary>
    /// Trae todos los reportes de una WO con sus propuestas eager-loaded.
    /// Útil para que el admin consolide propuestas en el presupuesto.
    /// </summary>
    Task<IReadOnlyList<InspectionReport>> GetByWorkOrderWithProposalsAsync(Guid workOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Áreas omitidas (IsSkipped=true) en la última orden no cancelada del vehículo,
    /// con Area y WorkOrder eager-loaded. Vacío si la última visita cubrió todo.
    /// Alimenta el aviso "quedó sin inspeccionar" en la ficha del vehículo y al crear una orden.
    /// </summary>
    Task<IReadOnlyList<InspectionReport>> GetSkippedForVehicleLastOrderAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
