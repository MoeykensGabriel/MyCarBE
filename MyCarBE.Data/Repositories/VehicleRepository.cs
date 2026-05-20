using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
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
        => await _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate, cancellationToken);

    public async Task<bool> LicensePlateExistsAsync(string licensePlate, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate && v.Id != excludeId, cancellationToken);

    public async Task<bool> VINExistsAsync(string vin, CancellationToken cancellationToken = default)
        => await _context.Vehicles.AnyAsync(v => v.VIN == vin, cancellationToken);

    public async Task<bool> VINExistsAsync(string vin, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Vehicles.AnyAsync(v => v.VIN == vin && v.Id != excludeId, cancellationToken);

    public async Task<PagedResult<Vehicle>> SearchPagedAsync(
        string? search, Guid? customerId, Guid? fleetId,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Vehicles.AsQueryable();

        // Filtro por dueño
        if (customerId.HasValue)
            query = query.Where(v => v.CustomerId == customerId);
        else if (fleetId.HasValue)
            query = query.Where(v => v.FleetId == fleetId);

        // Búsqueda por patente, marca o modelo
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(v =>
                v.LicensePlate.ToLower().Contains(term) ||
                v.Brand.ToLower().Contains(term)        ||
                v.Model.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Vehicle>(items, totalCount, page, pageSize);
    }
}
