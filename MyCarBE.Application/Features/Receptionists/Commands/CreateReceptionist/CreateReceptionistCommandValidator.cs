using FluentValidation;

namespace MyCarBE.Application.Features.Receptionists.Commands.CreateReceptionist;

public class CreateReceptionistCommandValidator : AbstractValidator<CreateReceptionistCommand>
{
    public CreateReceptionistCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
    }
}
