using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.AddAdHocServiceToWorkOrder;

public class AddAdHocServiceToWorkOrderCommandValidator
    : AbstractValidator<AddAdHocServiceToWorkOrderCommand>
{
    public AddAdHocServiceToWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del servicio es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.")
            .LessThanOrEqualTo(99_999_999).WithMessage("El precio es demasiado alto.");

        RuleFor(x => x.EstimatedDurationMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("La duración no puede ser negativa.")
            .LessThanOrEqualTo(1440).WithMessage("La duración no puede superar 24 horas (1440 min).");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 999).WithMessage("La cantidad debe estar entre 1 y 999.");
    }
}
