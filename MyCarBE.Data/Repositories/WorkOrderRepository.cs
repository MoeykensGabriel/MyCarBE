using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AppDbContext context) : base(context) { }

    public async Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry)
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Photos.Where(p => !p.IsDeleted))
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.AssignedMechanic)
            // CatalogService ya no se incluye: la duración ahora vive en
            // EstimatedDurationMinutesSnapshot del WorkOrderService (soporta ad-hoc).
            .Include(w => w.Photos.Where(p => !p.IsDeleted && p.WorkOrderServiceId == null))
            .Include(w => w.StatusChanges.OrderBy(sc => sc.ChangedAt))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<WorkOrder?> GetWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Include(w => w.Services.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetAllPagedAsync(WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders, status, search, ownerType, page, pageSize, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByVehicleIdPagedAsync(Guid vehicleId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.VehicleId == vehicleId), status, search, ownerType, page, pageSize, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByCustomerIdAtEntryPagedAsync(Guid customerId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.CustomerIdAtEntry == customerId), status, search, ownerType, page, pageSize, cancellationToken);

    public Task<PagedResult<WorkOrder>> GetByFleetIdAtEntryPagedAsync(Guid fleetId, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken = default)
        => PagedAsync(_context.WorkOrders.Where(w => w.FleetIdAtEntry == fleetId), status, search, ownerType, page, pageSize, cancellationToken);

    public async Task<WorkOrderService?> GetServiceByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
        => await _context.WorkOrderServices
            .Include(s => s.WorkOrder)
            .Include(s => s.AssignedMechanic)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);

    public async Task<IReadOnlyList<WorkOrderService>> GetServicesByMechanicAsync(
        Guid mechanicId,
        WorkOrderServiceAssignmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrderServices
            .Include(s => s.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .Where(s => s.AssignedMechanicId == mechanicId);

        if (status.HasValue)
            query = query.Where(s => s.AssignmentStatus == status.Value);
        else
            // Por defecto: solo activos (Pending o Accepted)
            query = query.Where(s =>
                s.AssignmentStatus == WorkOrderServiceAssignmentStatus.Pending ||
                s.AssignmentStatus == WorkOrderServiceAssignmentStatus.Accepted);

        return await query
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    private static async Task<PagedResult<WorkOrder>> PagedAsync(
        IQueryable<WorkOrder> query, WorkOrderStatus? status, string? search, WorkOrderOwnerType? ownerType, int page, int pageSize, CancellationToken cancellationToken)
    {
        query = query
            .Include(w => w.Vehicle)
            .Include(w => w.CustomerAtEntry)
            .Include(w => w.FleetAtEntry);

        if (status.HasValue)
            query = query.Where(w => w.CurrentStatus == status.Value);

        if (ownerType.HasValue)
            query = ownerType.Value == WorkOrderOwnerType.Fleet
                ? query.Where(w => w.FleetIdAtEntry != null)
                : query.Where(w => w.CustomerIdAtEntry != null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(w =>
                w.Vehicle.LicensePlate.ToLower().Contains(term) ||
                (w.CustomerAtEntry != null &&
                    (w.CustomerAtEntry.FirstName + " " + w.CustomerAtEntry.LastName).ToLower().Contains(term)) ||
                (w.FleetAtEntry != null &&
                    w.FleetAtEntry.CompanyName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrder>(items, totalCount, page, pageSize);
    }
}
