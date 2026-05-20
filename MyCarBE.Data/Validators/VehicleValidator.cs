using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class VehicleValidator : AbstractValidator<Vehicle>
{
    public VehicleValidator()
    {
        RuleFor(v => v.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            .MaximumLength(20);

        RuleFor(v => v.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100);

        RuleFor(v => v.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100);

        RuleFor(v => v.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(v => v.VIN)
            .Length(17).WithMessage("VIN must be exactly 17 characters.")
            .When(v => !string.IsNullOrWhiteSpace(v.VIN));

        RuleFor(v => v.CurrentMileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage cannot be negative.");

        RuleFor(v => v.RegistrationHolderFirstName)
            .NotEmpty().WithMessage("Registration holder first name is required.")
            .MaximumLength(100);

        RuleFor(v => v.RegistrationHolderLastName)
            .NotEmpty().WithMessage("Registration holder last name is required.")
            .MaximumLength(100);

        RuleFor(v => v.RegistrationHolderDocumentNumber)
            .NotEmpty().WithMessage("Registration holder document number is required.")
            .MaximumLength(50);

        // XOR: exactamente uno de los dos debe estar seteado
        RuleFor(v => v)
            .Must(v => v.CustomerId.HasValue ^ v.FleetId.HasValue)
            .WithMessage("A vehicle must belong to either a Customer or a Fleet, not both or neither.")
            .WithName("Ownership");
    }
}
