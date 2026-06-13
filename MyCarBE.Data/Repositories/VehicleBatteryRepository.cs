using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleBatteryRepository : Repository<VehicleBattery>, IVehicleBatteryRepository
{
    public VehicleBatteryRepository(AppDbContext context) : base(context) { }

    public async Task<VehicleBattery?> GetActiveByVehicleAsync(
        Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleBatteries
            .FirstOrDefaultAsync(
                b => b.VehicleId == vehicleId && b.IsActive,
                cancellationToken);

    public async Task<IReadOnlyList<VehicleBattery>> GetByVehicleAsync(
        Guid vehicleId, bool includeReplaced = false, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleBatteries
            .AsNoTracking()
            .Include(b => b.Checks.OrderBy(c => c.CheckedOn))
            .Where(b => b.VehicleId == vehicleId);

        if (!includeReplaced)
            query = query.Where(b => b.IsActive);

        return await query
            .OrderByDescending(b => b.InstalledOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleBattery>> GetActiveByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default)
    {
        if (customerId is null && fleetId is null)
            return Array.Empty<VehicleBattery>();

        return await _context.VehicleBatteries
            .AsNoTracking()
            .Include(b => b.Checks)
            .Include(b => b.Vehicle)
            .Where(b => b.IsActive &&
                ((customerId != null && b.Vehicle.CustomerId == customerId) ||
                 (fleetId    != null && b.Vehicle.FleetId    == fleetId)))
            .ToListAsync(cancellationToken);
    }
}
