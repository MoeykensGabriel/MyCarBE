using FluentValidation;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.StartTrip;

public class StartTripCommandValidator : AbstractValidator<StartTripCommand>
{
    public StartTripCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverDocument).NotEmpty().MaximumLength(30);
        RuleFor(x => x.StartKm)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(9_999_999);
    }
}
