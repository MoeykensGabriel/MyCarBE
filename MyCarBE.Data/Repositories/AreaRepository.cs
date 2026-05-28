using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class AreaRepository : Repository<Area>, IAreaRepository
{
    public AreaRepository(AppDbContext context) : base(context) { }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Areas.AnyAsync(a => a.Name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<bool> NameExistsAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Areas.AnyAsync(a => a.Name.ToLower() == name.ToLower() && a.Id != excludeId, cancellationToken);

    public async Task<IReadOnlyList<Area>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _context.Areas.AsQueryable();
        if (!includeInactive)
            query = query.Where(a => a.IsActive);

        return await query
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Area>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet();
        return await _context.Areas
            .Where(a => idSet.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }
}
