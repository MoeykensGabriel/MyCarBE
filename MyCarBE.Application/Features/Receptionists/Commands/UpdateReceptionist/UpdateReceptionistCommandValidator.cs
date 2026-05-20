using FluentValidation;

namespace MyCarBE.Application.Features.Receptionists.Commands.UpdateReceptionist;

public class UpdateReceptionistCommandValidator : AbstractValidator<UpdateReceptionistCommand>
{
    public UpdateReceptionistCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
