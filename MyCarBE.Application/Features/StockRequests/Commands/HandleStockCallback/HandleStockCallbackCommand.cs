using MediatR;

namespace MyCarBE.Application.Features.StockRequests.Commands.HandleStockCallback;

/// <summary>
/// Disparado por GestionPGB cuando confirma la entrega de un pedido al taller.
/// El payload llega con los items que el depósito reservó y entregó.
/// </summary>
public record HandleStockCallbackCommand(
    string                              LicensePlate,
    Guid                                WorkshopOrderId,
    string                              Status,
    DateTime                            DeliveredAt,
    IReadOnlyList<CallbackItemPayload>  Items) : IRequest<Unit>;

public record CallbackItemPayload(
    string ProductCode,
    int    RequestedQuantity,
    int    DeliveredQuantity,
    string Status);
