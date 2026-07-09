using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrderServices.Commands.CompleteServiceAsWorkshop;

public class CompleteServiceAsWorkshopCommandValidator : AbstractValidator<CompleteServiceAsWorkshopCommand>
{
    public CompleteServiceAsWorkshopCommandValidator()
    {
        RuleFor(x => x.WorkOrderServiceId).NotEmpty();

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Las notas son obligatorias para finalizar el servicio.")
            .MinimumLength(10).WithMessage("Las notas deben tener al menos 10 caracteres.")
            .MaximumLength(2000);
    }
}
