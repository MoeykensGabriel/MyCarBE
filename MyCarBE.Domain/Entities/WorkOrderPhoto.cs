using MyCarBE.Domain.Common;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Domain.Entities;

public class WorkOrderPhoto : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    // Nullable — null = foto general de la orden; con valor = foto vinculada a un servicio específico
    public Guid? WorkOrderServiceId { get; set; }
    public WorkOrderService? WorkOrderService { get; set; }

    public PhotoType PhotoType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime TakenAt { get; set; }
}
