using FluentValidation;

namespace MyCarBE.Application.Features.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("La matrícula es obligatoria.")
            .MaximumLength(20);

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es obligatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1);

        RuleFor(x => x.CurrentMileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

        When(x => x.VIN is not null, () =>
            RuleFor(x => x.VIN).MaximumLength(17));

        RuleFor(x => x.RegistrationHolderFirstName)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.RegistrationHolderLastName)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.RegistrationHolderDocumentNumber)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue ^ x.FleetId.HasValue)
            .WithName("Ownership")
            .WithMessage("El vehículo debe pertenecer exactamente a un Cliente o a una Flota, no a ambos ni a ninguno.");
    }
}
