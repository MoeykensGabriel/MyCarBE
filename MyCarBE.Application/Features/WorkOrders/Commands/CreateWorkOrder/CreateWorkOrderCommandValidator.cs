using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("El Id del vehículo es obligatorio.");

        RuleFor(x => x.MileageAtEntry)
            .InclusiveBetween(0, 9_999_999)
            .WithMessage("El kilometraje debe estar entre 0 y 9.999.999.");

        When(x => !string.IsNullOrEmpty(x.CustomerNote), () =>
            RuleFor(x => x.CustomerNote)
                .MaximumLength(1000).WithMessage("La nota del cliente no puede superar 1000 caracteres."));

        // ServiceReason: obligatorio a partir de S3-14 — el wizard de intake siempre lo envía.
        RuleFor(x => x.ServiceReason)
            .NotEmpty().WithMessage("El motivo de visita es obligatorio.")
            .MaximumLength(2000).WithMessage("El motivo de visita no puede superar 2000 caracteres.");
    }
}
