using MediatR;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SetSaleCondition;

/// <summary>
/// La oficina define la condición de venta de los repuestos de la orden, antes de la
/// aprobación. Viaja al depósito (GestionPGB) cuando el cliente aprueba el presupuesto:
///   - CuentaCorriente: sin datos extra.
///   - OrdenDeCompra:   PurchaseOrderNumber obligatorio.
///   - Contado:         DepositAmount obligatorio (la seña; puede ser 0 y el depósito
///                      verá "SIN SEÑA" — información valiosa para no pedir en vano).
/// Condition null limpia la condición.
/// </summary>
public record SetSaleConditionCommand(
    Guid           WorkOrderId,
    SaleCondition? Condition,
    string?        PurchaseOrderNumber,
    decimal?       DepositAmount
) : IRequest<WorkOrderDetailDto>;
