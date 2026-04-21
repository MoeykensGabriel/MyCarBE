using FluentValidation;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Validators;

public class WorkOrderServiceValidator : AbstractValidator<WorkOrderService>
{
    public WorkOrderServiceValidator()
    {
        RuleFor(s => s.WorkOrderId)
            .NotEmpty().WithMessage("WorkOrderId is required.");

        RuleFor(s => s.CatalogServiceId)
            .NotEmpty().WithMessage("CatalogServiceId is required.");

        RuleFor(s => s.NameSnapshot)
            .NotEmpty().WithMessage("Service name snapshot is required.")
            .MaximumLength(200);

        RuleFor(s => s.DescriptionSnapshot)
            .NotEmpty().WithMessage("Service description snapshot is required.")
            .MaximumLength(1000);

        RuleFor(s => s.PriceSnapshot)
            .GreaterThanOrEqualTo(0).WithMessage("Price snapshot cannot be negative.");

        RuleFor(s => s.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}
