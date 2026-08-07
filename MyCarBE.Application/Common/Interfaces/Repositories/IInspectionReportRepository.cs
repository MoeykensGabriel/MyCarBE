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
    /// Áreas del vehículo que quedaron postergadas y NO se volvieron a inspeccionar después,
    /// con Area y WorkOrder eager-loaded (los de la postergación vigente). Vacío si no arrastra nada.
    ///
    /// Es una deuda del VEHÍCULO, no una foto de la última visita: el taller cierra la orden
    /// postergando lo que no llegó a mirar, hace el trabajo, y abre OTRA orden para lo omitido.
    /// Por eso la deuda tiene que sobrevivir a la apertura de esa orden nueva y limpiarse sola
    /// recién cuando el área se inspecciona de verdad.
    /// </summary>
    Task<IReadOnlyList<InspectionReport>> GetPendingSkippedForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ¿Este área del vehículo está en deuda? Es el permiso de escritura del canal de
    /// inspección tardía: solo un área que quedó postergada se puede inspeccionar con la
    /// inspección inicial ya cerrada.
    ///
    /// Se apoya en GetPendingSkippedForVehicleAsync a propósito, para que "estar en deuda"
    /// signifique exactamente lo mismo al leer (el aviso) que al escribir (el permiso).
    /// </summary>
    Task<bool> IsAreaPendingForVehicleAsync(Guid vehicleId, Guid areaId, CancellationToken cancellationToken = default);
}
