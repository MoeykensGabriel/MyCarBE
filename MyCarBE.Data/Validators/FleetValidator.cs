using FluentValidation;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class FleetValidator : AbstractValidator<Fleet>
{
    public FleetValidator()
    {
        RuleFor(f => f.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        // Misma regla central que el command validator (formato + checksum AFIP)
        RuleFor(f => (string?)f.TaxId)
            .NotEmpty().WithMessage("Tax ID (CUIT) is required.")
            .MaximumLength(20)
            .MustBeValidArgentinaCuit()
            .When(f => !string.IsNullOrWhiteSpace(f.TaxId))
            .OverridePropertyName(nameof(Fleet.TaxId));

        RuleFor(f => f.Address)
            .MaximumLength(300)
            .When(f => !string.IsNullOrWhiteSpace(f.Address));
    }
}
