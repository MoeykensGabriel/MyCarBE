using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.DecideAdditionalItems;

public class DecideAdditionalItemsCommandValidator : AbstractValidator<DecideAdditionalItemsCommand>
{
    public DecideAdditionalItemsCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x)
            .Must(x => x.ApprovedServiceIds.Count + x.RejectedServiceIds.Count +
                       x.ApprovedPartIds.Count + x.RejectedPartIds.Count > 0)
            .WithMessage("Tenés que decidir al menos un item adicional.");
    }
}
