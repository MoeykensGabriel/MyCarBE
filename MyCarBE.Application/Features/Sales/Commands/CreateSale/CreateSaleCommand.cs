using MediatR;
using MyCarBE.Application.Features.Sales.DTOs;

namespace MyCarBE.Application.Features.Sales.Commands.CreateSale;

public record CreateSaleItemInput(
    string? ProductCode,
    string  Name,
    decimal UnitPrice,
    int     Quantity
);

/// <summary>
/// Registra una venta de repuestos de mostrador. El vendedor NO viene en el comando:
/// se toma del usuario logueado en el handler.
/// </summary>
public record CreateSaleCommand(
    Guid?                              CustomerId,
    Guid?                              FleetId,
    IReadOnlyList<CreateSaleItemInput> Items
) : IRequest<SaleDto>;
