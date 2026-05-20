using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class WorkOrderValidator : AbstractValidator<WorkOrder>
{
    public WorkOrderValidator()
    {
        RuleFor(w => w.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");

        RuleFor(w => w.MileageAtEntry)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage at entry cannot be negative.");

        RuleFor(w => w.CustomerNote)
            .MaximumLength(1000)
            .When(w => !string.IsNullOrWhiteSpace(w.CustomerNote));

        RuleFor(w => w.TechnicianNote)
            .MaximumLength(1000)
            .When(w => !string.IsNullOrWhiteSpace(w.TechnicianNote));

        RuleFor(w => w.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount cannot be negative.");
    }
}
