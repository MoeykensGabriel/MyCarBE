using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Repositories;

public class PartsStockRequestRepository : Repository<PartsStockRequest>, IPartsStockRequestRepository
{
    public PartsStockRequestRepository(AppDbContext context) : base(context) { }

    public async Task<PartsStockRequest?> GetByWorkOrderIdAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => await _context.PartsStockRequests
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.WorkOrderId == workOrderId, cancellationToken);

    public async Task<PartsStockRequest?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.PartsStockRequests
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .Include(r => r.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PartsStockRequest>> GetAllFilteredAsync(
        StockRequestStatus? status,
        string? licensePlate,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PartsStockRequests
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .Include(r => r.WorkOrder)
                .ThenInclude(w => w.Vehicle)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(licensePlate))
        {
            var plate = licensePlate.Trim().ToUpper();
            query = query.Where(r => r.LicensePlateSnapshot.ToUpper().Contains(plate));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PartsStockRequestItem?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken = default)
        => await _context.PartsStockRequestItems
            .Include(i => i.PartsStockRequest)
                .ThenInclude(r => r.Items.Where(it => !it.IsDeleted))
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
}
