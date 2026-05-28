using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Repuesto sugerido por el mecánico en su inspección — todavía NO es parte del presupuesto.
/// El admin decide cuáles propuestas convertir en WorkOrderPart al armar el presupuesto.
///
/// EstimatedUnitPrice es opcional: el mecánico puede no saber el precio del repuesto
/// (lo carga la oficina al armar la cotización, con info de proveedores).
/// ProductCode es opcional: si el mecánico lo conoce, ayuda a buscarlo en GestionPGB.
/// </summary>
public class InspectionReportProposedPart : BaseEntity
{
    public Guid InspectionReportId { get; set; }
    public InspectionReport InspectionReport { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;

    /// <summary>Código del proveedor / GestionPGB. Opcional.</summary>
    public string? ProductCode { get; set; }

    /// <summary>Precio unitario estimado. Opcional — el mecánico puede no saberlo.</summary>
    public decimal? EstimatedUnitPrice { get; set; }
}
