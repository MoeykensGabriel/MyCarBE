using FluentValidation;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;

public class ChangeWorkOrderStatusCommandValidator : AbstractValidator<ChangeWorkOrderStatusCommand>
{
    public ChangeWorkOrderStatusCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El Id de la orden es obligatorio.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("El estado solicitado no es válido.");

        // Note is required at the FluentValidation level too (belt-and-suspenders with domain)
        When(x => x.NewStatus == WorkOrderStatus.Cancelled, () =>
            RuleFor(x => x.Note)
                .NotEmpty().WithMessage("Se requiere una nota al cancelar una orden de trabajo.")
                .MaximumLength(1000));

        When(x => !string.IsNullOrEmpty(x.Note), () =>
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("La nota no puede superar 1000 caracteres."));
    }
}
