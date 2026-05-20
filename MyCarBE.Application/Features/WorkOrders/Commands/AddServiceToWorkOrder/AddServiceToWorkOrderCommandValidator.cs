using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;

public class AddServiceToWorkOrderCommandValidator : AbstractValidator<AddServiceToWorkOrderCommand>
{
    public AddServiceToWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.CatalogServiceId).NotEmpty();
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 999)
            .WithMessage("La cantidad debe estar entre 1 y 999.");
    }
}
