using FluentValidation;

namespace MyCarBE.Application.Features.Mechanics.Commands.AssignAreasToMechanic;

public class AssignAreasToMechanicCommandValidator : AbstractValidator<AssignAreasToMechanicCommand>
{
    public AssignAreasToMechanicCommandValidator()
    {
        RuleFor(x => x.MechanicId).NotEmpty();
        RuleFor(x => x.AreaIds)
            .NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("La lista de áreas no puede contener duplicados.");
    }
}
