using FluentValidation;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

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

        // XOR: TimeBased requiere DueDate, MileageBased requiere DueMileage
        RuleFor(a => a.DueDate)
            .NotNull().WithMessage("DueDate is required for TimeBased alerts.")
            .When(a => a.AlertType == AlertType.TimeBased);

        RuleFor(a => a.DueMileage)
            .Null().WithMessage("DueMileage must be null for TimeBased alerts.")
            .When(a => a.AlertType == AlertType.TimeBased);

        RuleFor(a => a.DueMileage)
            .NotNull().WithMessage("DueMileage is required for MileageBased alerts.")
            .GreaterThan(0).WithMessage("DueMileage must be greater than 0.")
            .When(a => a.AlertType == AlertType.MileageBased);

        RuleFor(a => a.DueDate)
            .Null().WithMessage("DueDate must be null for MileageBased alerts.")
            .When(a => a.AlertType == AlertType.MileageBased);
    }
}
