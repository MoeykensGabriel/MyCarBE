using Microsoft.EntityFrameworkCore;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Repositories;

public class WorkOrderApprovalTokenRepository : Repository<WorkOrderApprovalToken>, IWorkOrderApprovalTokenRepository
{
    public WorkOrderApprovalTokenRepository(AppDbContext context) : base(context) { }

    public async Task<WorkOrderApprovalToken?> GetValidByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.WorkOrderApprovalTokens
            .FirstOrDefaultAsync(
                t => t.Token == token && !t.IsUsed && !t.IsDeleted && t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    // Sin filtro de IsUsed/ExpiresAt — para mostrar estado real al cliente (expirado, ya usado, etc.)
    public async Task<WorkOrderApprovalToken?> GetByTokenValueAsync(string token, CancellationToken cancellationToken = default)
        => await _context.WorkOrderApprovalTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<WorkOrderApprovalToken?> GetActiveByWorkOrderIdAsync(Guid workOrderId, CancellationToken cancellationToken = default)
        => await _context.WorkOrderApprovalTokens
            .Where(t => t.WorkOrderId == workOrderId && !t.IsUsed && !t.IsDeleted && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
