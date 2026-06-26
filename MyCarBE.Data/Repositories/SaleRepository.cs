using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Models;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public async Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Fleet)
            .Include(s => s.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<PagedResult<Sale>> GetPagedAsync(
        Guid? customerId,
        Guid? fleetId,
        Guid? sellerUserId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Fleet)
            .Include(s => s.Items.Where(i => !i.IsDeleted))
            .AsQueryable();

        if (customerId.HasValue)   query = query.Where(s => s.CustomerId == customerId);
        if (fleetId.HasValue)      query = query.Where(s => s.FleetId == fleetId);
        if (sellerUserId.HasValue) query = query.Where(s => s.SellerUserId == sellerUserId);
        if (from.HasValue)         query = query.Where(s => s.CreatedAt >= from.Value);
        if (to.HasValue)           query = query.Where(s => s.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Sale>(items, totalCount, page, pageSize);
    }
}
