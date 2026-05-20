using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class DashboardRepository : IDashboardRepository
{
    // Fallback si por alguna razón la fila de WorkshopSettings no existe
    // (no debería pasar — el seeder la crea al arrancar).
    private const int DefaultPhysicalCapacity = 6;

    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardRawData> GetRawDataAsync(CancellationToken cancellationToken = default)
    {
        var now       = DateTime.UtcNow;
        var todayUtc  = now.Date;
        var weekStart = todayUtc.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Query 1: agrega conteos y revenue en una sola pasada
        var aggregates = await _context.WorkOrders
            .GroupBy(_ => 1)
            .Select(g => new
            {
                // counts by status
                Received         = g.Count(w => w.CurrentStatus == WorkOrderStatus.Received),
                Diagnosing       = g.Count(w => w.CurrentStatus == WorkOrderStatus.Diagnosing),
                AwaitingApproval = g.Count(w => w.CurrentStatus == WorkOrderStatus.AwaitingApproval),
                Approved         = g.Count(w => w.CurrentStatus == WorkOrderStatus.Approved),
                InProgress       = g.Count(w => w.CurrentStatus == WorkOrderStatus.InProgress),
                Completed        = g.Count(w => w.CurrentStatus == WorkOrderStatus.Completed),
                Delivered        = g.Count(w => w.CurrentStatus == WorkOrderStatus.Delivered),
                Cancelled        = g.Count(w => w.CurrentStatus == WorkOrderStatus.Cancelled),

                // period counts
                OrdersToday     = g.Count(w => w.CreatedAt >= todayUtc),
                OrdersThisWeek  = g.Count(w => w.CreatedAt >= weekStart),
                OrdersThisMonth = g.Count(w => w.CreatedAt >= monthStart),

                // revenue — solo Completed y Delivered
                RevenueToday = g
                    .Where(w => w.CreatedAt >= todayUtc &&
                                (w.CurrentStatus == WorkOrderStatus.Completed ||
                                 w.CurrentStatus == WorkOrderStatus.Delivered))
                    .Sum(w => (decimal?)w.TotalAmount) ?? 0m,

                RevenueThisMonth = g
                    .Where(w => w.CreatedAt >= monthStart &&
                                (w.CurrentStatus == WorkOrderStatus.Completed ||
                                 w.CurrentStatus == WorkOrderStatus.Delivered))
                    .Sum(w => (decimal?)w.TotalAmount) ?? 0m,
            })
            .FirstOrDefaultAsync(cancellationToken);

        // Query 2: últimas 5 órdenes con navegaciones para el DTO
        var recentOrders = await _context.WorkOrders
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            .OrderByDescending(w => w.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Query 3: vehículos físicamente asociados a órdenes activas.
        // Definimos "activas" como todo menos Delivered y Cancelled.
        var vehiclesInShop = await _context.WorkOrders
            .CountAsync(w =>
                w.CurrentStatus != WorkOrderStatus.Delivered &&
                w.CurrentStatus != WorkOrderStatus.Cancelled,
                cancellationToken);

        // Carga por mecánico activo — la armamos en 2 queries simples
        // y combinamos en memoria. La versión con sub-selects dentro del
        // constructor del DTO no la podía traducir EF Core.

        // 4a. Mecánicos activos
        var activeMechanics = await _context.Mechanics
            .Where(m => m.IsActive)
            .Select(m => new { m.Id, m.FirstName, m.LastName })
            .ToListAsync(cancellationToken);

        // 4b. Agregado de tareas / minutos pendientes, agrupado por mecánico
        var loadAgg = await _context.WorkOrderServices
            .Where(s =>
                s.AssignedMechanicId != null &&
                s.AssignmentStatus != Domain.Enums.WorkOrderServiceAssignmentStatus.Completed &&
                s.WorkOrder.CurrentStatus != WorkOrderStatus.Delivered &&
                s.WorkOrder.CurrentStatus != WorkOrderStatus.Cancelled)
            .GroupBy(s => s.AssignedMechanicId!.Value)
            .Select(g => new
            {
                MechanicId = g.Key,
                TaskCount  = g.Count(),
                // Usamos el snapshot — soporta servicios del catálogo y ad-hoc por igual.
                Minutes    = g.Sum(s => s.EstimatedDurationMinutesSnapshot * s.Quantity),
            })
            .ToListAsync(cancellationToken);

        var loadMap = loadAgg.ToDictionary(x => x.MechanicId);

        // 4c. Combinamos: todos los mecánicos activos aparecen,
        // los que no tienen tareas asignadas quedan con 0 / 0.
        var mechanicsLoad = activeMechanics
            .Select(m =>
            {
                loadMap.TryGetValue(m.Id, out var l);
                return new MechanicLoadRaw(
                    MechanicId:       m.Id,
                    FirstName:        m.FirstName,
                    LastName:         m.LastName,
                    PendingTaskCount: l?.TaskCount ?? 0,
                    PendingMinutes:   l?.Minutes   ?? 0
                );
            })
            .OrderBy(x => x.PendingMinutes)  // mecánico más libre primero
            .ThenBy(x => x.LastName)
            .ToList();

        var totalPendingMinutes = mechanicsLoad.Sum(m => m.PendingMinutes);

        // Capacity sale ahora de la tabla WorkshopSettings (singleton).
        // Fallback al default si por alguna razón no existe la fila.
        var settings = await _context.WorkshopSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        var physicalCapacity = settings?.PhysicalCapacity ?? DefaultPhysicalCapacity;

        // ── Widget: aprobaciones por vencer en próximas 24hs ────────────────
        // Tokens activos (no usados, no vencidos) cuya WO sigue esperando aprobación.
        var expiringCutoff = now.AddHours(24);
        var expiringApprovals = await _context.WorkOrderApprovalTokens
            .Where(t =>
                !t.IsUsed &&
                t.ExpiresAt > now &&
                t.ExpiresAt <= expiringCutoff &&
                t.WorkOrder.CurrentStatus == WorkOrderStatus.AwaitingApproval)
            .OrderBy(t => t.ExpiresAt) // los más próximos a vencer primero
            .Take(5)
            .Select(t => new ExpiringApprovalRaw(
                t.WorkOrderId,
                t.WorkOrder.Vehicle.Brand,
                t.WorkOrder.Vehicle.Model,
                t.WorkOrder.Vehicle.LicensePlate,
                t.WorkOrder.CustomerAtEntry != null ? t.WorkOrder.CustomerAtEntry.FirstName : null,
                t.WorkOrder.CustomerAtEntry != null ? t.WorkOrder.CustomerAtEntry.LastName  : null,
                t.WorkOrder.FleetAtEntry    != null ? t.WorkOrder.FleetAtEntry.CompanyName  : null,
                t.ExpiresAt))
            .ToListAsync(cancellationToken);

        // ── Widget: top 5 mecánicos por servicios finalizados este mes ──────
        // Solo servicios completados en el mes actual. Ordenado descendente.
        var topMechanicsAgg = await _context.WorkOrderServices
            .Where(s =>
                s.AssignmentStatus == Domain.Enums.WorkOrderServiceAssignmentStatus.Completed &&
                s.CompletedAt != null &&
                s.CompletedAt >= monthStart &&
                s.AssignedMechanicId != null)
            .GroupBy(s => s.AssignedMechanicId!.Value)
            .Select(g => new { MechanicId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topMechanicIds = topMechanicsAgg.Select(x => x.MechanicId).ToList();
        var mechanicsInfo = await _context.Mechanics
            .Where(m => topMechanicIds.Contains(m.Id))
            .Select(m => new { m.Id, m.FirstName, m.LastName })
            .ToListAsync(cancellationToken);

        var mechanicInfoMap = mechanicsInfo.ToDictionary(m => m.Id);
        var topMechanics = topMechanicsAgg
            .Select(x =>
            {
                mechanicInfoMap.TryGetValue(x.MechanicId, out var info);
                return new TopMechanicRaw(
                    x.MechanicId,
                    info?.FirstName ?? "—",
                    info?.LastName  ?? "",
                    x.Count
                );
            })
            .ToList();

        // ── Widget: top 5 servicios más vendidos este mes ───────────────────
        // Cuenta servicios del catálogo en WOs creadas este mes (excluyendo canceladas).
        // Los servicios ad-hoc (sin CatalogServiceId) no entran al ranking del catálogo.
        var topServicesAgg = await _context.WorkOrderServices
            .Where(s =>
                s.CatalogServiceId != null &&
                s.WorkOrder.CreatedAt >= monthStart &&
                s.WorkOrder.CurrentStatus != WorkOrderStatus.Cancelled)
            .GroupBy(s => s.CatalogServiceId!.Value)
            .Select(g => new { CatalogServiceId = g.Key, Total = g.Sum(s => s.Quantity) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topServiceIds = topServicesAgg.Select(x => x.CatalogServiceId).ToList();
        var catalogInfo = await _context.CatalogServices
            .Where(c => topServiceIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var catalogMap = catalogInfo.ToDictionary(c => c.Id);
        var topServices = topServicesAgg
            .Select(x =>
            {
                catalogMap.TryGetValue(x.CatalogServiceId, out var info);
                return new TopServiceRaw(
                    x.CatalogServiceId,
                    info?.Name ?? "—",
                    x.Total
                );
            })
            .ToList();

        // ── Widget: vehículos en Completed esperando ser retirados ──────────
        // Para encontrar cuándo se completó cada WO, miramos el último cambio
        // de estado a Completed. Lo hacemos en 2 pasos para que EF lo traduzca:
        // 1) traer las WOs Completed con sus StatusChanges
        // 2) calcular CompletedAt en memoria y ordenar
        var completedWos = await _context.WorkOrders
            .Where(w => w.CurrentStatus == WorkOrderStatus.Completed)
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            .Include(w => w.StatusChanges)
            .ToListAsync(cancellationToken);

        var vehiclesToPickup = completedWos
            .Select(w =>
            {
                var lastCompletedChange = w.StatusChanges
                    .Where(sc => sc.ToStatus == WorkOrderStatus.Completed)
                    .OrderByDescending(sc => sc.ChangedAt)
                    .FirstOrDefault();

                var completedAt = lastCompletedChange?.ChangedAt ?? w.UpdatedAt;

                var customerName = w.CustomerAtEntry != null
                    ? $"{w.CustomerAtEntry.FirstName} {w.CustomerAtEntry.LastName}".Trim()
                    : (w.FleetAtEntry?.CompanyName ?? "—");

                var customerPhone = w.CustomerAtEntry?.Phone ?? w.FleetAtEntry?.Phone;

                return new VehicleToPickupRaw(
                    w.Id,
                    w.Vehicle.Brand,
                    w.Vehicle.Model,
                    w.Vehicle.LicensePlate,
                    customerName,
                    customerPhone,
                    completedAt
                );
            })
            .OrderBy(v => v.CompletedAt)   // los más antiguos primero (más urgentes)
            .Take(8)
            .ToList();

        var result = new DashboardRawData
        {
            RecentOrders    = recentOrders,
            OrdersToday     = aggregates?.OrdersToday     ?? 0,
            OrdersThisWeek  = aggregates?.OrdersThisWeek  ?? 0,
            OrdersThisMonth = aggregates?.OrdersThisMonth ?? 0,
            RevenueToday    = aggregates?.RevenueToday    ?? 0m,
            RevenueThisMonth = aggregates?.RevenueThisMonth ?? 0m,
            VehiclesInShop      = vehiclesInShop,
            PhysicalCapacity    = physicalCapacity,
            TotalPendingMinutes = totalPendingMinutes,
            MechanicsLoad       = mechanicsLoad,
            ExpiringApprovals   = expiringApprovals,
            TopMechanics        = topMechanics,
            TopServices         = topServices,
            VehiclesToPickup    = vehiclesToPickup,
        };

        if (aggregates is not null)
        {
            result.CountsByStatus = new Dictionary<WorkOrderStatus, int>
            {
                { WorkOrderStatus.Received,         aggregates.Received         },
                { WorkOrderStatus.Diagnosing,        aggregates.Diagnosing       },
                { WorkOrderStatus.AwaitingApproval, aggregates.AwaitingApproval },
                { WorkOrderStatus.Approved,         aggregates.Approved         },
                { WorkOrderStatus.InProgress,       aggregates.InProgress       },
                { WorkOrderStatus.Completed,        aggregates.Completed        },
                { WorkOrderStatus.Delivered,        aggregates.Delivered        },
                { WorkOrderStatus.Cancelled,        aggregates.Cancelled        },
            };
        }

        return result;
    }
}
