using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderServiceDto(
    Guid    Id,
    /// <summary>
    /// Null cuando es un servicio ad-hoc (puntual) que no vive en el catálogo.
    /// En ese caso los datos están solo en los snapshots.
    /// </summary>
    Guid?   CatalogServiceId,
    string  NameSnapshot,
    string  DescriptionSnapshot,
    decimal PriceSnapshot,
    int     Quantity,
    decimal Subtotal,        // PriceSnapshot * Quantity — computed for frontend convenience

    /// <summary>
    /// Duración estimada por unidad. Snapshot — se toma al agregar el servicio
    /// y no cambia después aunque el catálogo se actualice.
    /// </summary>
    int     EstimatedDurationMinutes,

    // Asignación al mecánico
    Guid?   AssignedMechanicId,
    string? AssignedMechanicName,         // visible solo para Admin (lo filtra el handler si hace falta)
    WorkOrderServiceAssignmentStatus AssignmentStatus,
    DateTime? AcceptedAt,
    DateTime? CompletedAt,
    string? MechanicNotes,
    string? MechanicFindings,

    /// <summary>
    /// Área del taller a la que pertenece este servicio (Motor, Frenos, etc.).
    /// El FE lo usa para emparejar la foto "antes" (de la inspección del mismo área)
    /// con la foto "después" (que sube el mecánico al cerrar el servicio).
    /// </summary>
    Guid?   AreaId,
    string? AreaName
);
