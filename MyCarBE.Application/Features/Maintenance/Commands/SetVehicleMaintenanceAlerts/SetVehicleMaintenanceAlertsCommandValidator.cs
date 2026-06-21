using FluentValidation;

namespace MyCarBE.Application.Features.Maintenance.Commands.SetVehicleMaintenanceAlerts;

public class SetVehicleMaintenanceAlertsCommandValidator
    : AbstractValidator<SetVehicleMaintenanceAlertsCommand>
{
    public SetVehicleMaintenanceAlertsCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("El id del vehículo es obligatorio.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("La lista de alertas es obligatoria (puede ir vacía).");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.IntervalKm)
                .GreaterThan(0).WithMessage("El intervalo en km debe ser mayor a 0.")
                .When(i => i.IntervalKm.HasValue);

            item.RuleFor(i => i.IntervalMonths)
                .GreaterThan(0).WithMessage("El intervalo en meses debe ser mayor a 0.")
                .When(i => i.IntervalMonths.HasValue);

            item.RuleFor(i => i)
                .Must(i => i.IntervalKm.HasValue || i.IntervalMonths.HasValue)
                .WithMessage("Cada alerta necesita al menos un intervalo (km o meses).");

            item.RuleFor(i => i.Title)
                .MaximumLength(200);

            item.RuleFor(i => i.Description)
                .MaximumLength(1000)
                .When(i => !string.IsNullOrWhiteSpace(i.Description));
        });
    }
}
