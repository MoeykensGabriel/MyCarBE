using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Models;

/// <summary>
/// Snapshot de carga por mecánico activo, calculado directamente en SQL.
/// </summary>
public record MechanicLoadRaw(
    Guid   MechanicId,
    string FirstName,
    string LastName,
    int    PendingTaskCount,
    int    PendingMinutes
);

/// <summary>
/// Internal model used between IDashboardRepository and the query handler.
/// Not exposed outside the Application layer.
/// </summary>
public class DashboardRawData
{
    public Dictionary<WorkOrderStatus, int> CountsByStatus { get; set; } = new();
    public int     OrdersToday      { get; set; }
    public int     OrdersThisWeek   { get; set; }
    public int     OrdersThisMonth  { get; set; }
    public decimal RevenueToday     { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public IReadOnlyList<WorkOrder> RecentOrders { get; set; } = [];

    // ── Carga del taller (capacidad operativa) ───────────────────────────────
    /// <summary>Bahías ocupadas: vehículos InProgress + Completed (presentes físicamente).</summary>
    public int VehiclesInShop      { get; set; }
    /// <summary>Subconjunto de VehiclesInShop: Completed esperando retiro (sin trabajo activo).</summary>
    public int VehiclesAwaitingPickup { get; set; }
    public int PhysicalCapacity    { get; set; } = 6;
    public int TotalPendingMinutes { get; set; }
    public IReadOnlyList<MechanicLoadRaw> MechanicsLoad { get; set; } = [];

    // ── Widgets laterales ────────────────────────────────────────────────────
    public IReadOnlyList<ExpiringApprovalRaw> ExpiringApprovals { get; set; } = [];
    public IReadOnlyList<TopMechanicRaw>      TopMechanics      { get; set; } = [];
    public IReadOnlyList<TopServiceRaw>       TopServices       { get; set; } = [];
    public IReadOnlyList<VehicleToPickupRaw>  VehiclesToPickup  { get; set; } = [];

    /// <summary>Serie de ingresos mes a mes, del más viejo al más nuevo, sin huecos.</summary>
    public IReadOnlyList<MonthlyRevenueRaw>   MonthlyRevenue    { get; set; } = [];
    /// <summary>Ranking de recepcionistas por órdenes registradas en el mes.</summary>
    public IReadOnlyList<TopReceptionistRaw>  TopReceptionists  { get; set; } = [];
}

/// <summary>
/// Un mes de la serie de ingresos. Los meses sin facturación viajan igual con 0:
/// si se omitieran, el gráfico pegaría dos meses no consecutivos y la tendencia
/// se leería mal.
/// </summary>
public record MonthlyRevenueRaw(
    int     Year,
    int     Month,
    decimal Revenue,
    int     OrdersCount
);

/// <summary>
/// Cuántas órdenes registró cada recepcionista en el período.
/// </summary>
public record TopReceptionistRaw(
    Guid   ReceptionistId,
    string FirstName,
    string LastName,
    int    RegisteredCount
);

public record ExpiringApprovalRaw(
    Guid     WorkOrderId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,
    string?  CustomerFirstName,
    string?  CustomerLastName,
    string?  FleetCompanyName,
    DateTime ExpiresAt
);

public record TopMechanicRaw(
    Guid   MechanicId,
    string FirstName,
    string LastName,
    int    CompletedCount
);

public record TopServiceRaw(
    Guid   CatalogServiceId,
    string Name,
    int    TimesUsed
);

public record VehicleToPickupRaw(
    Guid     WorkOrderId,
    string   VehicleBrand,
    string   VehicleModel,
    string   VehicleLicensePlate,
    string   CustomerName,
    string?  CustomerPhone,
    DateTime CompletedAt
);
