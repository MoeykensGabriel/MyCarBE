namespace MyCarBE.Domain.Enums;

/// <summary>
/// Condición de venta del cliente para los repuestos de la orden. La define la
/// oficina antes de la aprobación y viaja al depósito (GestionPGB) junto con el
/// pedido de repuestos — es el criterio del depósito para comprar o no:
///   - CuentaCorriente: cliente de confianza, el depósito pide directo.
///   - OrdenDeCompra:   acompañada del número de OC (PurchaseOrderNumber).
///   - Contado:         acompañada del importe de la seña (DepositAmount);
///                      el depósito la corrobora por fuera del sistema.
/// Los valores deben coincidir textualmente con el enum SaleCondition de GestionPGB
/// (viajan como string en el payload).
/// </summary>
public enum SaleCondition
{
    CuentaCorriente = 0,
    OrdenDeCompra   = 1,
    Contado         = 2
}
