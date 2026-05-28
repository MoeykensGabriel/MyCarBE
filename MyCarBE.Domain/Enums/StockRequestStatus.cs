namespace MyCarBE.Domain.Enums;

/// <summary>
/// Estado agregado del pedido completo al Sistema de Stock — lo que ve la oficina en la
/// pantalla de seguimiento. Se calcula a partir de los estados individuales de los items
/// vía PartsStockRequest.RecomputeStatus(), y se denormaliza para queries rápidas.
/// </summary>
public enum StockRequestStatus
{
    /// <summary>Pedido recién enviado al depósito, todavía no respondió.</summary>
    PendingReview = 0,

    /// <summary>El depósito confirmó que falta al menos un repuesto y hay que comprarlo.</summary>
    HasShortages = 1,

    /// <summary>Todos los repuestos avanzaron — comprados o en camino — sin faltantes pendientes.</summary>
    InProgress = 2,

    /// <summary>Todos los repuestos fueron entregados a los mecánicos. Listo para coordinar turno.</summary>
    Ready = 3,
}
