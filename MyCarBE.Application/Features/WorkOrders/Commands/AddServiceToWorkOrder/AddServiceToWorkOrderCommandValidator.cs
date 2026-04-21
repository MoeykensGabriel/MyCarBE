using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;

public class AddServiceToWorkOrderCommandValidator : AbstractValidator<AddServiceToWorkOrderCommand>
{
    public AddServiceToWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.CatalogServiceId).NotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
    }
}
