using FluentValidation;

namespace MyCarBE.Application.Features.Sales.Commands.CreateSale;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue ^ x.FleetId.HasValue)
            .WithMessage("La venta tiene que ser a un cliente O a una flota (exactamente uno).");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta tiene que tener al menos un repuesto.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Name)
                .NotEmpty().WithMessage("El nombre del repuesto es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

            item.RuleFor(i => i.ProductCode!)
                .MaximumLength(100).WithMessage("El código no puede superar 100 caracteres.")
                .When(i => !string.IsNullOrWhiteSpace(i.ProductCode));

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.")
                .LessThanOrEqualTo(99_999_999).WithMessage("El precio es demasiado alto.");

            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, 9999).WithMessage("La cantidad debe estar entre 1 y 9999.");
        });
    }
}
