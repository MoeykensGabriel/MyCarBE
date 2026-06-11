using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleMileageReadingRepository : Repository<VehicleMileageReading>, IVehicleMileageReadingRepository
{
    public VehicleMileageReadingRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<VehicleMileageReading>> GetLatestByVehicleAsync(
        Guid vehicleId, int take, CancellationToken cancellationToken = default)
        => await _context.VehicleMileageReadings
            .AsNoTracking()
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
