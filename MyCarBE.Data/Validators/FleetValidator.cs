using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class FleetValidator : AbstractValidator<Fleet>
{
    public FleetValidator()
    {
        RuleFor(f => f.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(f => f.TaxId)
            .NotEmpty().WithMessage("Tax ID (CUIT) is required.")
            .MaximumLength(20)
            .Matches(@"^\d{2}-\d{8}-\d{1}$").WithMessage("TaxId must follow the CUIT format: XX-XXXXXXXX-X.");

        RuleFor(f => f.Address)
            .MaximumLength(300)
            .When(f => !string.IsNullOrWhiteSpace(f.Address));
    }
}
