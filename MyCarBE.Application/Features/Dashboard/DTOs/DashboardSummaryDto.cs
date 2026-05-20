using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.Dashboard.DTOs;

public record OrdersByStatusDto(
    int Received,
    int Diagnosing,
    int AwaitingApproval,
    int Approved,
    int InProgress,
    int Completed,
    int Delivered,
    int Cancelled
);

/// <summary>
/// Carga de trabajo pendiente de un mecánico activo. "Pending" en este contexto
/// significa servicios asignados que aún no fueron completados.
/// </summary>
public record MechanicLoadDto(
    Guid    MechanicId,
    string  FullName,
    int     PendingTaskCount,
    int     PendingMinutes
);

/// <summary>
/// Vista de carga del taller en tiempo real. Permite al admin responder
/// "¿tengo capacidad para esto?" sin caminar al taller ni preguntar a nadie.
/// </summary>
public record WorkshopLoadDto(
    /// <summary>Vehículos físicamente asociados a órdenes activas (no Delivered/Cancelled).</summary>
    int VehiclesInShop,

    /// <summary>Capacidad física del taller (configurable en appsettings).</summary>
    int PhysicalCapacity,

    /// <summary>Total de minutos pendientes de trabajo en todo el taller.</summary>
    int TotalPendingMinutes,

    /// <summary>Carga por mecánico activo (ordenado de menor a mayor).</summary>
    IReadOnlyList<MechanicLoadDto> MechanicsLoad
);

/// <summary>
/// Aprobación pendiente cuyo token está por vencer en las próximas N horas.
/// Pensado para que el admin contacte al cliente / regenere el link a tiempo.
/// </summary>
public record ExpiringApprovalDto(
    Guid     WorkOrderId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,
    string?  CustomerName,
    DateTime ExpiresAt,
    int      HoursLeft
);

/// <summary>
/// Ranking de mecánicos por servicios finalizados en el período actual.
/// </summary>
public record TopMechanicDto(
    Guid   MechanicId,
    string FullName,
    int    CompletedCount
);

/// <summary>
/// Ranking de servicios del catálogo por cantidad de veces usados en el período.
/// </summary>
public record TopServiceDto(
    Guid   CatalogServiceId,
    string Name,
    int    TimesUsed
);

/// <summary>
/// WO en estado Completed esperando que el cliente retire el vehículo.
/// Ordenadas por antigüedad de finalización (más vieja primero).
/// </summary>
public record VehicleToPickupDto(
    Guid     WorkOrderId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,
    string   CustomerName,
    string?  CustomerPhone,
    DateTime CompletedAt,
    int      DaysWaiting
);

public record DashboardSummaryDto(
    // Semáforo principal — órdenes que necesitan atención inmediata
    int PendingApprovals,

    // Órdenes activas (todo excepto Delivered y Cancelled)
    int ActiveOrders,

    // Desglose por estado
    OrdersByStatusDto OrdersByStatus,

    // Carga operativa del taller (capacidad, vehículos, mecánicos)
    WorkshopLoadDto WorkshopLoad,

    // Ingresos de órdenes Completed o Delivered
    decimal RevenueToday,
    decimal RevenueThisMonth,

    // Conteos por período (todas las órdenes, cualquier estado)
    int OrdersToday,
    int OrdersThisWeek,
    int OrdersThisMonth,

    // Últimas 5 órdenes creadas
    IReadOnlyList<WorkOrderSummaryDto> RecentOrders,

    // ── Widgets de columna lateral ───────────────────────────────────────────
    /// <summary>Tokens activos que vencen en las próximas 24 hs.</summary>
    IReadOnlyList<ExpiringApprovalDto> ExpiringApprovals,
    /// <summary>Top 5 mecánicos por servicios finalizados en el mes actual.</summary>
    IReadOnlyList<TopMechanicDto> TopMechanics,
    /// <summary>Top 5 servicios más vendidos en el mes actual.</summary>
    IReadOnlyList<TopServiceDto> TopServices,
    /// <summary>Vehículos en Completed esperando ser retirados, ordenados por antigüedad.</summary>
    IReadOnlyList<VehicleToPickupDto> VehiclesToPickup
);
