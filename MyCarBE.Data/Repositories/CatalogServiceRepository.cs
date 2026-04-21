using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class CatalogServiceRepository : Repository<CatalogService>, ICatalogServiceRepository
{
    public CatalogServiceRepository(AppDbContext context) : base(context) { }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        => await _context.CatalogServices
            .AnyAsync(c => c.Name == name, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.CatalogServices
            .AnyAsync(c => c.Name == name && c.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<CatalogService>> GetAllWithInactiveAsync(CancellationToken cancellationToken = default)
        => await _context.CatalogServices
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CatalogService>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.CatalogServices
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
}
