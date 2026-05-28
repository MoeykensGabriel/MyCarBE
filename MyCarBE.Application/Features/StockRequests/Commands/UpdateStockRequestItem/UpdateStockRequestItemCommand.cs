using MediatR;
using MyCarBE.Application.Features.StockRequests.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.Commands.UpdateStockRequestItem;

/// <summary>
/// Disparado por el webhook de GestionPGB cada vez que un item avanza en su flujo
/// (lo compraron, está en camino, llegó, etc.). Actualiza el estado del item y
/// recalcula el estado agregado del pedido.
/// </summary>
public record UpdateStockRequestItemCommand(
    Guid                    ItemId,
    StockRequestItemStatus  NewStatus,
    string?                 Notes) : IRequest<StockRequestDto>;
