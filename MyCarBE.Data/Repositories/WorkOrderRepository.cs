using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AppDbContext context) : base(context) { }

    public async Task<WorkOrder?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Include(w => w.Services.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Photos.Where(p => !p.IsDeleted))
            .Include(w => w.Photos.Where(p => !p.IsDeleted && p.WorkOrderServiceId == null))
            .Include(w => w.StatusChanges.OrderBy(sc => sc.ChangedAt))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<WorkOrder?> GetWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Include(w => w.Services.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Where(w => w.VehicleId == vehicleId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetByCustomerIdAtEntryAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Where(w => w.CustomerIdAtEntry == customerId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> GetByFleetIdAtEntryAsync(Guid fleetId, CancellationToken cancellationToken = default)
        => await _context.WorkOrders
            .Where(w => w.FleetIdAtEntry == fleetId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
}
