using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class MechanicRepository : Repository<Mechanic>, IMechanicRepository
{
    public MechanicRepository(AppDbContext context) : base(context) { }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Mechanics
            .AnyAsync(m => m.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Mechanics
            .AnyAsync(m => m.Email == email && m.Id != excludeId, cancellationToken);

    public async Task<Mechanic?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
        => await _context.Mechanics
            .FirstOrDefaultAsync(m => m.ApplicationUserId == applicationUserId, cancellationToken);

    public async Task<IReadOnlyList<Mechanic>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.Mechanics
            .Where(m => m.IsActive)
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<Mechanic>> SearchPagedAsync(
        string? searchTerm,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Mechanics.AsQueryable();

        if (!includeInactive)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term)  ||
                m.Email.ToLower().Contains(term)     ||
                (m.Specialty != null && m.Specialty.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Mechanic>(items, totalCount, page, pageSize);
    }
}
