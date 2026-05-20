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
    public int VehiclesInShop      { get; set; }
    public int PhysicalCapacity    { get; set; } = 6;
    public int TotalPendingMinutes { get; set; }
    public IReadOnlyList<MechanicLoadRaw> MechanicsLoad { get; set; } = [];

    // ── Widgets laterales ────────────────────────────────────────────────────
    public IReadOnlyList<ExpiringApprovalRaw> ExpiringApprovals { get; set; } = [];
    public IReadOnlyList<TopMechanicRaw>      TopMechanics      { get; set; } = [];
    public IReadOnlyList<TopServiceRaw>       TopServices       { get; set; } = [];
    public IReadOnlyList<VehicleToPickupRaw>  VehiclesToPickup  { get; set; } = [];
}

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
