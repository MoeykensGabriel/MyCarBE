using FluentValidation;

namespace MyCarBE.Application.Features.VehicleTires.Commands.CreateVehicleTire;

public class CreateVehicleTireCommandValidator : AbstractValidator<CreateVehicleTireCommand>
{
    public CreateVehicleTireCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Position).IsInEnum();

        RuleFor(x => x.Brand)    .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model)    .NotEmpty().MaximumLength(100);
        RuleFor(x => x.SizeSpec) .NotEmpty().MaximumLength(50);

        RuleFor(x => x.InstalledAtKm).GreaterThanOrEqualTo(0);

        // 1.6mm es el mínimo legal; instalar nueva con menos no tiene sentido.
        // 25mm es un techo generoso para cubrir cualquier 4x4 / camión.
        RuleFor(x => x.InitialTreadDepthMm)
            .InclusiveBetween(1.6m, 25m);

        RuleFor(x => x.ExpectedLifeKm).GreaterThanOrEqualTo(0);

        RuleFor(x => x.InstalledOn)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("La fecha de instalación no puede ser futura.");
    }
}
