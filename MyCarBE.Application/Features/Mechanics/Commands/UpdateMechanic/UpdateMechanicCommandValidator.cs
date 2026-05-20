using FluentValidation;
using MyCarBE.Application.Common.Validation;

namespace MyCarBE.Application.Features.Mechanics.Commands.UpdateMechanic;

public class UpdateMechanicCommandValidator : AbstractValidator<UpdateMechanicCommand>
{
    public UpdateMechanicCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .MustBeValidArgentinaPhone()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Specialty).MaximumLength(200);
    }
}
