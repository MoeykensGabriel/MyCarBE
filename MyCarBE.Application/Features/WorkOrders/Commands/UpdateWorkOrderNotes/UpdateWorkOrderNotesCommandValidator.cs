using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderNotes;

public class UpdateWorkOrderNotesCommandValidator : AbstractValidator<UpdateWorkOrderNotesCommand>
{
    public UpdateWorkOrderNotesCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El Id de la orden es obligatorio.");

        When(x => !string.IsNullOrEmpty(x.CustomerNote), () =>
            RuleFor(x => x.CustomerNote)
                .MaximumLength(1000).WithMessage("La nota del cliente no puede superar 1000 caracteres."));

        When(x => !string.IsNullOrEmpty(x.TechnicianNote), () =>
            RuleFor(x => x.TechnicianNote)
                .MaximumLength(2000).WithMessage("La nota del técnico no puede superar 2000 caracteres."));
    }
}
