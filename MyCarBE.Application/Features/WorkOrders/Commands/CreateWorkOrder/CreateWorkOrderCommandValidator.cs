using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("El Id del vehículo es obligatorio.");

        RuleFor(x => x.MileageAtEntry)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

        When(x => x.CustomerNote is not null, () =>
            RuleFor(x => x.CustomerNote)
                .MaximumLength(1000).WithMessage("La nota del cliente no puede superar 1000 caracteres."));
    }
}
