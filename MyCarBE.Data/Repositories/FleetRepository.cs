using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class FleetRepository : Repository<Fleet>, IFleetRepository
{
    public FleetRepository(AppDbContext context) : base(context) { }

    public async Task<Fleet?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
        => await _context.Fleets
            .FirstOrDefaultAsync(f => f.TaxId == taxId, cancellationToken);

    public async Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default)
        => await _context.Fleets
            .AnyAsync(f => f.TaxId == taxId, cancellationToken);
}
