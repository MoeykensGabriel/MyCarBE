using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class VehicleOilServiceRepository : Repository<VehicleOilService>, IVehicleOilServiceRepository
{
    public VehicleOilServiceRepository(AppDbContext context) : base(context) { }

    public async Task<VehicleOilService?> GetLatestByVehicleAsync(
        Guid vehicleId, CancellationToken cancellationToken = default)
        => await _context.VehicleOilServices
            .AsNoTracking()
            .Where(o => o.VehicleId == vehicleId)
            // El más reciente: por fecha del cambio y, a igualdad, por fecha de carga.
            .OrderByDescending(o => o.ChangedOn)
            .ThenByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleOilService>> GetLatestByOwnerAsync(
        Guid? customerId, Guid? fleetId, CancellationToken cancellationToken = default)
    {
        if (customerId is null && fleetId is null)
            return Array.Empty<VehicleOilService>();

        // Traemos los cambios de los vehículos del dueño ordenados por recencia y nos
        // quedamos con el último de cada vehículo en memoria (volumen acotado).
        var services = await _context.VehicleOilServices
            .AsNoTracking()
            .Include(o => o.Vehicle)
            .Where(o =>
                (customerId != null && o.Vehicle.CustomerId == customerId) ||
                (fleetId    != null && o.Vehicle.FleetId    == fleetId))
            .OrderByDescending(o => o.ChangedOn)
            .ThenByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return services
            .GroupBy(o => o.VehicleId)
            .Select(g => g.First())
            .ToList();
    }
}
