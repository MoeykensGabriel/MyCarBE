namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado individual de cada repuesto dentro de un pedido al Sistema de Stock (GestionPGB).
/// Los valores se mapean directamente con WorkshopItemStatus de GestionPGB:
///   Available (0,1)  → tenemos stock reservado en depósito
///   Shortage (1) / NotFound (4) → ambos colapsan a Missing
///   PurchasedInTransit (2) → InTransit
///   Delivered (3) → Delivered
/// </summary>
public enum StockRequestItemStatus
{
    /// <summary>Pedido en preparación local — todavía no enviado al depósito.</summary>
    PendingReview = 0,

    /// <summary>El depósito tiene stock reservado para este pedido.</summary>
    Available = 1,

    /// <summary>El depósito necesita comprar este repuesto (o el código no existe en su catálogo).</summary>
    Missing = 2,

    /// <summary>El depósito ya compró y está despachando el repuesto al taller.</summary>
    InTransit = 3,

    /// <summary>El repuesto fue entregado al mecánico que lo necesitaba.</summary>
    Delivered = 4,
}
