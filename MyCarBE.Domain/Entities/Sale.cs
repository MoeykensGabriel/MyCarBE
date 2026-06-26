using MyCarBE.Domain.Common;

namespace MyCarBE.Domain.Entities;

/// <summary>
/// Venta de repuestos "de mostrador": NO cuelga de una orden ni de un vehículo. Siempre es a un
/// cliente registrado (particular XOR flota) y registra quién la vendió (el usuario logueado),
/// para liquidar comisiones. Solo repuestos — sin servicios, sin flujo de aprobación.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>A quién se le vende: cliente particular XOR flota (uno de los dos seteado).</summary>
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? FleetId { get; set; }
    public Fleet? Fleet { get; set; }

    /// <summary>Quién vende: el usuario (Admin/Recepcionista) logueado al crear la venta.</summary>
    public Guid SellerUserId { get; set; }

    /// <summary>
    /// Nombre del vendedor al momento de vender (snapshot). El nombre no vive en un solo lugar
    /// (el Recepcionista tiene ficha; el Admin solo email), así que se resuelve y congela acá.
    /// </summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>Suma de los subtotales de los ítems activos.</summary>
    public decimal TotalAmount { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    /// <summary>Recalcula el total sumando los subtotales de los ítems activos.</summary>
    public void RecalculateTotalAmount()
        => TotalAmount = Items.Where(i => !i.IsDeleted).Sum(i => i.Subtotal);
}
