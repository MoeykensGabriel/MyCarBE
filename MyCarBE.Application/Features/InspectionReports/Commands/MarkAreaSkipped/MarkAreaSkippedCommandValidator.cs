using FluentValidation;

namespace MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaSkipped;

public class MarkAreaSkippedCommandValidator : AbstractValidator<MarkAreaSkippedCommand>
{
    public MarkAreaSkippedCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.AreaId).NotEmpty();

        // El motivo es la constancia de por qué no se inspeccionó — no puede faltar.
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Indicá el motivo por el que se omite la inspección de esta área.")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres.")
            .MaximumLength(500);
    }
}
