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

    public async Task<Fleet?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Fleets
            .Include(f => f.Contacts)
            .Include(f => f.Vehicles)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<bool> TaxIdExistsAsync(string taxId, CancellationToken cancellationToken = default)
        => await _context.Fleets
            .AnyAsync(f => f.TaxId == taxId, cancellationToken);

    public async Task<bool> TaxIdExistsAsync(string taxId, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Fleets
            .AnyAsync(f => f.TaxId == taxId && f.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Fleet>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _context.Fleets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(f =>
                f.CompanyName.ToLower().Contains(term) ||
                f.TaxId.Contains(term));
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
