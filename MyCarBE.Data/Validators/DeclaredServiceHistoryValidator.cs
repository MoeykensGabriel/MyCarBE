using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class DeclaredServiceHistoryValidator : AbstractValidator<DeclaredServiceHistory>
{
    public DeclaredServiceHistoryValidator()
    {
        RuleFor(d => d.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");

        RuleFor(d => d.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000);

        RuleFor(d => d.Workshop)
            .MaximumLength(200)
            .When(d => !string.IsNullOrWhiteSpace(d.Workshop));

        RuleFor(d => d.MileageAtService)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage at service cannot be negative.")
            .When(d => d.MileageAtService.HasValue);

        RuleFor(d => d.Notes)
            .MaximumLength(1000)
            .When(d => !string.IsNullOrWhiteSpace(d.Notes));
    }
}
