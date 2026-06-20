using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class MaintenanceAlertRepository : Repository<MaintenanceAlert>, IMaintenanceAlertRepository
{
    public MaintenanceAlertRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MaintenanceAlert>> GetByVehicleIdAsync(
        Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.MaintenanceAlerts
            .Where(a => a.VehicleId == vehicleId)
            .OrderBy(a => a.ItemType)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MaintenanceAlert>> GetActiveByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default)
    {
        if (customerId is null && fleetId is null)
            return Array.Empty<MaintenanceAlert>();

        return await _context.MaintenanceAlerts
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a =>
                (customerId != null && a.Vehicle.CustomerId == customerId) ||
                (fleetId    != null && a.Vehicle.FleetId    == fleetId))
            .ToListAsync(cancellationToken);
    }
}
