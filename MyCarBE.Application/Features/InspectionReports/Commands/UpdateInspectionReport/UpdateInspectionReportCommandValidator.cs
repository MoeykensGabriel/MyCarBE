using FluentValidation;

namespace MyCarBE.Application.Features.InspectionReports.Commands.UpdateInspectionReport;

public class UpdateInspectionReportCommandValidator : AbstractValidator<UpdateInspectionReportCommand>
{
    public UpdateInspectionReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Findings)
            .NotEmpty().WithMessage("Describí los hallazgos cuando marcás que hay un problema.")
            .MaximumLength(4000)
            .When(x => x.HasIssue);

        RuleFor(x => x.Findings)
            .MaximumLength(4000)
            .When(x => !x.HasIssue);

        RuleForEach(x => x.ProposedServices).ChildRules(s =>
        {
            s.RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
            s.RuleFor(p => p.Description).MaximumLength(2000);
            s.RuleFor(p => p.EstimatedLaborCost).GreaterThanOrEqualTo(0);
            s.RuleFor(p => p.EstimatedDurationMinutes).GreaterThan(0).When(p => p.EstimatedDurationMinutes.HasValue);
        });

        RuleForEach(x => x.ProposedParts).ChildRules(p =>
        {
            p.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            p.RuleFor(x => x.Quantity).GreaterThan(0);
            p.RuleFor(x => x.ProductCode).MaximumLength(100);
            p.RuleFor(x => x.EstimatedUnitPrice).GreaterThanOrEqualTo(0).When(x => x.EstimatedUnitPrice.HasValue);
        });
    }
}
