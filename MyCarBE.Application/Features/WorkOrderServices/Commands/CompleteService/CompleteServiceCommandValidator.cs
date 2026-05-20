using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteService;

public class CompleteServiceCommandValidator : AbstractValidator<CompleteServiceCommand>
{
    public CompleteServiceCommandValidator()
    {
        RuleFor(x => x.WorkOrderServiceId).NotEmpty();

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Las notas son obligatorias para finalizar el servicio.")
            .MinimumLength(10).WithMessage("Las notas deben tener al menos 10 caracteres.")
            .MaximumLength(2000);

        RuleFor(x => x.Findings)
            .MaximumLength(2000);
    }
}
