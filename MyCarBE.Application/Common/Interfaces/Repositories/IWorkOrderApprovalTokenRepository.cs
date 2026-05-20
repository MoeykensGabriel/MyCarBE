using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Interfaces.Repositories;

public interface IWorkOrderApprovalTokenRepository : IRepository<WorkOrderApprovalToken>
{
    Task<WorkOrderApprovalToken?> GetValidByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<WorkOrderApprovalToken?> GetByTokenValueAsync(string token, CancellationToken cancellationToken = default);
    Task<WorkOrderApprovalToken?> GetActiveByWorkOrderIdAsync(Guid workOrderId, CancellationToken cancellationToken = default);
}
