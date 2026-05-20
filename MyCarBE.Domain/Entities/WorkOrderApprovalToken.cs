using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

public class WorkOrderApprovalToken : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsValid() => !IsUsed && !IsDeleted && ExpiresAt > DateTime.UtcNow;
}
