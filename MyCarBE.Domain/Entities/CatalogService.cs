using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

public class CatalogService : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}
