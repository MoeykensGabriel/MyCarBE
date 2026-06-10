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
}
