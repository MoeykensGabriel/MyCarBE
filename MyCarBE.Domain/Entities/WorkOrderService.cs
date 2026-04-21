using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

public class WorkOrderService : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public Guid CatalogServiceId { get; set; }
    public CatalogService CatalogService { get; set; } = null!;

    // Snapshots del catálogo al momento de agregar — no se propagan cambios posteriores
    public string NameSnapshot { get; set; } = string.Empty;
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal PriceSnapshot { get; set; }

    public int Quantity { get; set; } = 1;

    // Navegación
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();
}
