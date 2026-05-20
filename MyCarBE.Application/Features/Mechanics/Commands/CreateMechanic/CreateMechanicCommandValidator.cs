using FluentValidation;
using MyCarBE.Application.Common.Validation;

namespace MyCarBE.Application.Features.Mechanics.Commands.CreateMechanic;

public class CreateMechanicCommandValidator : AbstractValidator<CreateMechanicCommand>
{
    public CreateMechanicCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);

        // Phone es opcional en mecánicos. Si viene, validamos formato.
        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .MustBeValidArgentinaPhone()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Specialty).MaximumLength(200);
    }
}
