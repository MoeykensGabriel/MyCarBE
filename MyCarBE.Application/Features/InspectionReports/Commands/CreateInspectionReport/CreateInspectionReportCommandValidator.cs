using FluentValidation;

namespace MyCarBE.Application.Features.InspectionReports.Commands.CreateInspectionReport;

public class CreateInspectionReportCommandValidator : AbstractValidator<CreateInspectionReportCommand>
{
    public CreateInspectionReportCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.AreaId).NotEmpty();

        // Si encontró algo, debe describirlo. Si no hay problemas, Findings es opcional.
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
            s.RuleFor(p => p.EstimatedDays).GreaterThan(0).When(p => p.EstimatedDays.HasValue);
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
