using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class CatalogServiceValidator : AbstractValidator<CatalogService>
{
    public CatalogServiceValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(200);

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Service description is required.")
            .MaximumLength(1000);

        RuleFor(c => c.DefaultPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Default price cannot be negative.");

        RuleFor(c => c.EstimatedDurationMinutes)
            .GreaterThan(0).WithMessage("Estimated duration must be greater than 0.");
    }
}
