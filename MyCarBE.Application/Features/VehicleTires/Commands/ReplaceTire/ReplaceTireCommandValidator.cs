using FluentValidation;

namespace MyCarBE.Application.Features.VehicleTires.Commands.ReplaceTire;

public class ReplaceTireCommandValidator : AbstractValidator<ReplaceTireCommand>
{
    public ReplaceTireCommandValidator()
    {
        RuleFor(x => x.CurrentTireId).NotEmpty();
        RuleFor(x => x.ReplacedAtKm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReplacedOn)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("La fecha de reemplazo no puede ser futura.");

        RuleFor(x => x.NewBrand)   .NotEmpty().MaximumLength(100);
        RuleFor(x => x.NewModel)   .NotEmpty().MaximumLength(100);
        RuleFor(x => x.NewSizeSpec).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NewInitialTreadDepthMm).InclusiveBetween(1.6m, 25m);
        RuleFor(x => x.NewExpectedLifeKm).GreaterThanOrEqualTo(0);
    }
}
