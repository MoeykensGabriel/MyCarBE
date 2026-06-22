using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderServicePrice;

public class UpdateWorkOrderServicePriceCommandValidator
    : AbstractValidator<UpdateWorkOrderServicePriceCommand>
{
    public UpdateWorkOrderServicePriceCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("El id del servicio es obligatorio.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.")
            .LessThanOrEqualTo(99_999_999).WithMessage("El precio es demasiado alto.");
    }
}
