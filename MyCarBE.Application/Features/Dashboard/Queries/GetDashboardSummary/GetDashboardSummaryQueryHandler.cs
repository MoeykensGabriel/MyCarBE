using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Dashboard.DTOs;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IDashboardRepository _repository;
    private readonly IMapper              _mapper;

    public GetDashboardSummaryQueryHandler(IDashboardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var data = await _repository.GetRawDataAsync(cancellationToken);

        var counts = data.CountsByStatus;

        int Get(WorkOrderStatus s) => counts.TryGetValue(s, out var v) ? v : 0;

        var byStatus = new OrdersByStatusDto(
            Received:         Get(WorkOrderStatus.Received),
            Diagnosing:       Get(WorkOrderStatus.Diagnosing),
            AwaitingApproval: Get(WorkOrderStatus.AwaitingApproval),
            Approved:         Get(WorkOrderStatus.Approved),
            InProgress:       Get(WorkOrderStatus.InProgress),
            Completed:        Get(WorkOrderStatus.Completed),
            Delivered:        Get(WorkOrderStatus.Delivered),
            Cancelled:        Get(WorkOrderStatus.Cancelled)
        );

        var activeOrders = byStatus.Received
                         + byStatus.Diagnosing
                         + byStatus.AwaitingApproval
                         + byStatus.Approved
                         + byStatus.InProgress
                         + byStatus.Completed;

        var recentOrders = _mapper.Map<IReadOnlyList<WorkOrderSummaryDto>>(data.RecentOrders);

        // Carga del taller — capacidad ya viene leída del repo desde appsettings.
        var mechanicsLoad = data.MechanicsLoad
            .Select(m => new MechanicLoadDto(
                MechanicId:       m.MechanicId,
                FullName:         $"{m.FirstName} {m.LastName}".Trim(),
                PendingTaskCount: m.PendingTaskCount,
                PendingMinutes:   m.PendingMinutes))
            .ToList();

        var workshopLoad = new WorkshopLoadDto(
            VehiclesInShop:      data.VehiclesInShop,
            PhysicalCapacity:    data.PhysicalCapacity,
            TotalPendingMinutes: data.TotalPendingMinutes,
            MechanicsLoad:       mechanicsLoad);

        // ── Widgets laterales ────────────────────────────────────────────────
        var now = DateTime.UtcNow;

        var expiringApprovals = data.ExpiringApprovals
            .Select(a => new ExpiringApprovalDto(
                WorkOrderId:         a.WorkOrderId,
                VehicleBrand:        a.VehicleBrand,
                VehicleModel:        a.VehicleModel,
                VehicleLicensePlate: a.VehicleLicensePlate,
                CustomerName:        a.FleetCompanyName ??
                                     (a.CustomerFirstName != null
                                         ? $"{a.CustomerFirstName} {a.CustomerLastName}".Trim()
                                         : null),
                ExpiresAt:           a.ExpiresAt,
                HoursLeft:           Math.Max(0, (int)Math.Floor((a.ExpiresAt - now).TotalHours))))
            .ToList();

        var topMechanics = data.TopMechanics
            .Select(m => new TopMechanicDto(
                MechanicId:     m.MechanicId,
                FullName:       $"{m.FirstName} {m.LastName}".Trim(),
                CompletedCount: m.CompletedCount))
            .ToList();

        var topServices = data.TopServices
            .Select(s => new TopServiceDto(
                CatalogServiceId: s.CatalogServiceId,
                Name:             s.Name,
                TimesUsed:        s.TimesUsed))
            .ToList();

        var vehiclesToPickup = data.VehiclesToPickup
            .Select(v => new VehicleToPickupDto(
                WorkOrderId:         v.WorkOrderId,
                VehicleBrand:        v.VehicleBrand,
                VehicleModel:        v.VehicleModel,
                VehicleLicensePlate: v.VehicleLicensePlate,
                CustomerName:        v.CustomerName,
                CustomerPhone:       v.CustomerPhone,
                CompletedAt:         v.CompletedAt,
                DaysWaiting:         Math.Max(0, (int)Math.Floor((now - v.CompletedAt).TotalDays))))
            .ToList();

        return new DashboardSummaryDto(
            PendingApprovals:   byStatus.AwaitingApproval,
            ActiveOrders:       activeOrders,
            OrdersByStatus:     byStatus,
            WorkshopLoad:       workshopLoad,
            RevenueToday:       data.RevenueToday,
            RevenueThisMonth:   data.RevenueThisMonth,
            OrdersToday:        data.OrdersToday,
            OrdersThisWeek:     data.OrdersThisWeek,
            OrdersThisMonth:    data.OrdersThisMonth,
            RecentOrders:       recentOrders,
            ExpiringApprovals:  expiringApprovals,
            TopMechanics:       topMechanics,
            TopServices:        topServices,
            VehiclesToPickup:   vehiclesToPickup
        );
    }
}
