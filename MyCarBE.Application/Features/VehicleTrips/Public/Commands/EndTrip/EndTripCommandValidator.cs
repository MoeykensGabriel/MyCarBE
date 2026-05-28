using FluentValidation;

namespace MyCarBE.Application.Features.VehicleTrips.Public.Commands.EndTrip;

public class EndTripCommandValidator : AbstractValidator<EndTripCommand>
{
    public EndTripCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(64);
        RuleFor(x => x.EndKm).GreaterThanOrEqualTo(0).LessThanOrEqualTo(9_999_999);
    }
}
