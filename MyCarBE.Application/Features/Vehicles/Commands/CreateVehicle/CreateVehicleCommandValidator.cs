using System.Text.RegularExpressions;
using FluentValidation;
using MyCarBE.Application.Common.Validation;

namespace MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    // Mercosur: AB123CD | Nacional: ABC123  (espacios ignorados, case-insensitive)
    private static readonly Regex LicensePlateRegex =
        new(@"^([A-Z]{2}\d{3}[A-Z]{2}|[A-Z]{3}\d{3})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("La patente es obligatoria.")
            .Must(p => LicensePlateRegex.IsMatch(p?.Replace(" ", "") ?? ""))
            .WithMessage("Formato de patente inválido. Formatos aceptados: Mercosur (AB123CD) o Nacional (ABC123).");

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
            .InclusiveBetween(0, 9_999_999)
            .WithMessage("El kilometraje debe estar entre 0 y 9.999.999.");

        RuleFor(x => x.FuelType).IsInEnum().WithMessage("El tipo de combustible no es válido.");
        RuleFor(x => x.VehicleBodyType).IsInEnum().WithMessage("El tipo de carrocería no es válido.");
        RuleFor(x => x.VehicleUseType).IsInEnum().WithMessage("El tipo de uso no es válido.");
        RuleFor(x => x.RegistrationHolderDocumentType).IsInEnum().WithMessage("El tipo de documento del titular no es válido.");

        When(x => !string.IsNullOrEmpty(x.VIN), () =>
            RuleFor(x => x.VIN).MaximumLength(17).WithMessage("El VIN no puede superar 17 caracteres."));

        When(x => !string.IsNullOrEmpty(x.EngineNumber), () =>
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

        // Validación según el tipo de documento del titular registral
        RuleFor(x => (string?)x.RegistrationHolderDocumentNumber)
            .MustMatchDocumentType(x => x.RegistrationHolderDocumentType)
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationHolderDocumentNumber))
            .OverridePropertyName(nameof(CreateVehicleCommand.RegistrationHolderDocumentNumber));

        // XOR: exactly one of CustomerId / FleetId must be set
        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue ^ x.FleetId.HasValue)
            .WithName("Ownership")
            .WithMessage("El vehículo debe pertenecer exactamente a un Cliente o a una Flota, no a ambos ni a ninguno.");
    }
}
