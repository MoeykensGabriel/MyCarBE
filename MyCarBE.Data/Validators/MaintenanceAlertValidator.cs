using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class MaintenanceAlertValidator : AbstractValidator<MaintenanceAlert>
{
    public MaintenanceAlertValidator()
    {
        RuleFor(a => a.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");

        RuleFor(a => a.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(a => a.Description)
            .MaximumLength(1000)
            .When(a => !string.IsNullOrWhiteSpace(a.Description));

        // Al menos un intervalo (km y/o tiempo) — esto da el comportamiento "como el aceite".
        RuleFor(a => a)
            .Must(a => a.IntervalKm.HasValue || a.IntervalMonths.HasValue)
            .WithMessage("Configurá al menos un intervalo (km o meses).");

        RuleFor(a => a.IntervalKm)
            .GreaterThan(0).WithMessage("IntervalKm debe ser mayor a 0.")
            .When(a => a.IntervalKm.HasValue);

        RuleFor(a => a.IntervalMonths)
            .GreaterThan(0).WithMessage("IntervalMonths debe ser mayor a 0.")
            .When(a => a.IntervalMonths.HasValue);

        RuleFor(a => a.BaselineMileage)
            .GreaterThanOrEqualTo(0).WithMessage("BaselineMileage no puede ser negativo.");
    }
}
