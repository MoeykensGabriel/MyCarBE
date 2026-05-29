using FluentValidation;

namespace MyCarBE.Application.Features.VehicleTires.Commands.AddTireMeasurement;

public class AddTireMeasurementCommandValidator : AbstractValidator<AddTireMeasurementCommand>
{
    public AddTireMeasurementCommandValidator()
    {
        RuleFor(x => x.VehicleTireId).NotEmpty();

        RuleFor(x => x.MeasuredOn)
            .Must(d => d <= DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de medición no puede ser futura.");

        RuleFor(x => x.VehicleMileageAtMeasurement).GreaterThanOrEqualTo(0);

        // Profundidades: 0–25mm cubre desde calva hasta una cubierta industrial nueva.
        RuleFor(x => x.InnerDepthMm) .InclusiveBetween(0m, 25m);
        RuleFor(x => x.CenterDepthMm).InclusiveBetween(0m, 25m);
        RuleFor(x => x.OuterDepthMm) .InclusiveBetween(0m, 25m);

        RuleFor(x => x.Notes).MaximumLength(1000);

        // WorkOrderId es opcional, pero si viene no puede ser un Guid vacío.
        When(x => x.WorkOrderId.HasValue, () =>
            RuleFor(x => x.WorkOrderId!.Value)
                .NotEmpty()
                .WithMessage("La orden de trabajo indicada no es válida."));
    }
}
