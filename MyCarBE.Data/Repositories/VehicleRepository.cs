using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext context) : base(context) { }

    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, cancellationToken);

    public async Task<bool> LicensePlateExistsAsync(string licensePlate, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .AnyAsync(v => v.LicensePlate == licensePlate, cancellationToken);

    public async Task<bool> LicensePlateExistsAsync(string licensePlate, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .AnyAsync(v => v.LicensePlate == licensePlate && v.Id != excludeId, cancellationToken);

    public async Task<bool> VINExistsAsync(string vin, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .AnyAsync(v => v.VIN == vin, cancellationToken);

    public async Task<bool> VINExistsAsync(string vin, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .AnyAsync(v => v.VIN == vin && v.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .Where(v => v.CustomerId == customerId)
            .OrderBy(v => v.LicensePlate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByFleetIdAsync(Guid fleetId, CancellationToken cancellationToken = default)
        => await _context.Vehicles
            .Where(v => v.FleetId == fleetId)
            .OrderBy(v => v.LicensePlate)
            .ToListAsync(cancellationToken);
}
