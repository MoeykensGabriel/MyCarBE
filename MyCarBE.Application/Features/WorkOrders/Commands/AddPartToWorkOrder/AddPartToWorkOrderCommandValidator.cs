using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;

public class AddPartToWorkOrderCommandValidator : AbstractValidator<AddPartToWorkOrderCommand>
{
    public AddPartToWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del repuesto es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        When(x => !string.IsNullOrWhiteSpace(x.ProductCode), () =>
            RuleFor(x => x.ProductCode!)
                .MaximumLength(100).WithMessage("El código de producto no puede superar 100 caracteres."));

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.")
            .LessThanOrEqualTo(99_999_999).WithMessage("El precio es demasiado alto.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 9999).WithMessage("La cantidad debe estar entre 1 y 9999.");
    }
}
