using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Una línea de venta: un repuesto vendido. Espeja a <see cref="WorkOrderPart"/> pero sin tier
/// ni estado de aprobación (la venta es directa). El código es texto libre (barcode de proveedor).
/// </summary>
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    /// <summary>Código de barras / proveedor del repuesto. Opcional (texto libre).</summary>
    public string? ProductCode { get; set; }

    /// <summary>Descripción del repuesto (lo que se factura).</summary>
    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>Subtotal (precio × cantidad). Calculado — no se persiste.</summary>
    public decimal Subtotal => UnitPrice * Quantity;
}
