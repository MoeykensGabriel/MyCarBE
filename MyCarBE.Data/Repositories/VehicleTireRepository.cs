using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class VehicleTireRepository : Repository<VehicleTire>, IVehicleTireRepository
{
    public VehicleTireRepository(AppDbContext context) : base(context) { }

    public async Task<VehicleTire?> GetActiveByPositionAsync(
        Guid vehicleId, TirePosition position, CancellationToken cancellationToken = default)
        => await _context.VehicleTires
            .FirstOrDefaultAsync(
                t => t.VehicleId == vehicleId
                  && t.Position  == position
                  && t.IsActive,
                cancellationToken);

    public async Task<IReadOnlyList<VehicleTire>> GetByVehicleAsync(
        Guid vehicleId, bool includeReplaced = false, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleTires
            .AsNoTracking()
            .Include(t => t.Measurements.OrderBy(m => m.MeasuredOn))
            .Where(t => t.VehicleId == vehicleId);

        if (!includeReplaced)
            query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.Position)
            .ThenByDescending(t => t.InstalledOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleTire?> GetByIdWithMeasurementsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.VehicleTires
            .Include(t => t.Measurements.OrderBy(m => m.MeasuredOn))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
