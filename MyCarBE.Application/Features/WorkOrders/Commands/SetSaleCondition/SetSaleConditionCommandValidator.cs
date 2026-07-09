using FluentValidation;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.SetSaleCondition;

public class SetSaleConditionCommandValidator : AbstractValidator<SetSaleConditionCommand>
{
    public SetSaleConditionCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();

        RuleFor(x => x.Condition).IsInEnum().When(x => x.Condition.HasValue);

        // OC: el número es la constancia que el depósito le muestra al proveedor.
        RuleFor(x => x.PurchaseOrderNumber)
            .NotEmpty().WithMessage("Indicá el número de la orden de compra.")
            .MaximumLength(100)
            .When(x => x.Condition == SaleCondition.OrdenDeCompra);

        // Contado: la seña es el dato que decide si el depósito pide o no.
        // 0 es válido — significa "no señó nada" y el depósito lo ve explícito.
        RuleFor(x => x.DepositAmount)
            .NotNull().WithMessage("Indicá el importe de la seña (0 si no señó).")
            .GreaterThanOrEqualTo(0)
            .When(x => x.Condition == SaleCondition.Contado);
    }
}
