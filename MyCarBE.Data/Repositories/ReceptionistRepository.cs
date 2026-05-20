using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class ReceptionistRepository : Repository<Receptionist>, IReceptionistRepository
{
    public ReceptionistRepository(AppDbContext context) : base(context) { }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Receptionists
            .AnyAsync(r => r.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid excludeId, CancellationToken cancellationToken = default)
        => await _context.Receptionists
            .AnyAsync(r => r.Email == email && r.Id != excludeId, cancellationToken);

    public async Task<Receptionist?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
        => await _context.Receptionists
            .FirstOrDefaultAsync(r => r.ApplicationUserId == applicationUserId, cancellationToken);

    public async Task<PagedResult<Receptionist>> SearchPagedAsync(
        string? searchTerm,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Receptionists.AsQueryable();

        if (!includeInactive)
            query = query.Where(r => r.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(r =>
                r.FirstName.ToLower().Contains(term) ||
                r.LastName.ToLower().Contains(term)  ||
                r.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Receptionist>(items, totalCount, page, pageSize);
    }
}
