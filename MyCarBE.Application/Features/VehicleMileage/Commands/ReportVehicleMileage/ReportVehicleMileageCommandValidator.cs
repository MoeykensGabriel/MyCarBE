using FluentValidation;

namespace MyCarBE.Application.Features.VehicleMileage.Commands.ReportVehicleMileage;

public class ReportVehicleMileageCommandValidator : AbstractValidator<ReportVehicleMileageCommand>
{
    public ReportVehicleMileageCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("El id del vehículo es obligatorio.");

        // Mismo rango que CurrentMileage en CreateVehicle. La monotonía contra la
        // última lectura se valida en el handler (necesita el vehículo cargado).
        RuleFor(x => x.Mileage)
            .InclusiveBetween(0, 9_999_999)
            .WithMessage("El kilometraje debe estar entre 0 y 9.999.999.");
    }
}
