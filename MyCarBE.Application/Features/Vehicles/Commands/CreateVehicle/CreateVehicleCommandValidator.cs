using FluentValidation;

namespace MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("La matrícula es obligatoria.")
            .MaximumLength(20).WithMessage("La matrícula no puede superar 20 caracteres.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es obligatoria.")
            .MaximumLength(100).WithMessage("La marca no puede superar 100 caracteres.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es obligatorio.")
            .MaximumLength(100).WithMessage("El modelo no puede superar 100 caracteres.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"El año debe estar entre 1900 y {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.CurrentMileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

        When(x => x.VIN is not null, () =>
            RuleFor(x => x.VIN).MaximumLength(17).WithMessage("El VIN no puede superar 17 caracteres."));

        When(x => x.EngineNumber is not null, () =>
            RuleFor(x => x.EngineNumber).MaximumLength(50).WithMessage("El número de motor no puede superar 50 caracteres."));

        RuleFor(x => x.RegistrationHolderFirstName)
            .NotEmpty().WithMessage("El nombre del titular de la cédula verde es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.RegistrationHolderLastName)
            .NotEmpty().WithMessage("El apellido del titular de la cédula verde es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.RegistrationHolderDocumentNumber)
            .NotEmpty().WithMessage("El documento del titular es obligatorio.")
            .MaximumLength(50);

        // XOR: exactly one of CustomerId / FleetId must be set
        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue ^ x.FleetId.HasValue)
            .WithName("Ownership")
            .WithMessage("El vehículo debe pertenecer exactamente a un Cliente o a una Flota, no a ambos ni a ninguno.");
    }
}
