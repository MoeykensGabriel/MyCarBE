using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class VehicleTripRepository : Repository<VehicleTrip>, IVehicleTripRepository
{
    public VehicleTripRepository(AppDbContext context) : base(context) { }

    public async Task<Vehicle?> GetVehicleByTripTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Vehicles.FirstOrDefaultAsync(v => v.TripToken == token, cancellationToken);

    public async Task<VehicleTrip?> GetOpenTripForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleTrips
            .Where(t => t.VehicleId == vehicleId && t.Status == VehicleTripStatus.Open)
            .OrderByDescending(t => t.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<VehicleTrip?> GetLastClosedTripForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleTrips
            .Where(t => t.VehicleId == vehicleId && t.Status != VehicleTripStatus.Open && t.EndKm != null)
            .OrderByDescending(t => t.EndedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleTrip>> GetTripsByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleTrips
            .AsNoTracking()
            .Where(t => t.VehicleId == vehicleId)
            .OrderByDescending(t => t.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleTrip>> GetOpenTripsByFleetAsync(Guid fleetId, CancellationToken cancellationToken = default)
        => await _context.VehicleTrips
            .AsNoTracking()
            .Include(t => t.Vehicle)
            .Where(t => t.Status == VehicleTripStatus.Open && t.Vehicle.FleetId == fleetId)
            .OrderBy(t => t.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> TripTokenExistsAsync(string token, CancellationToken cancellationToken = default)
        => await _context.Vehicles.AnyAsync(v => v.TripToken == token, cancellationToken);
}
