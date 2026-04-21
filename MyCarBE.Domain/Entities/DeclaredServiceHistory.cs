using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

public class DeclaredServiceHistory : BaseEntity
{
    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public DateTime DeclaredDate { get; set; }          // Fecha aproximada declarada por el cliente
    public string Description { get; set; } = string.Empty;
    public string? Workshop { get; set; }               // Texto libre — nombre del taller externo
    public int? MileageAtService { get; set; }
    public string? Notes { get; set; }
}
