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
            s.RuleFor(p => p.EstimatedDurationMinutes).GreaterThan(0).When(p => p.EstimatedDurationMinutes.HasValue);
        });

        RuleForEach(x => x.ProposedParts).ChildRules(p =>
        {
            p.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            p.RuleFor(x => x.Quantity).GreaterThan(0);
            p.RuleFor(x => x.ProductCode).MaximumLength(100);
            p.RuleFor(x => x.EstimatedUnitPrice).GreaterThanOrEqualTo(0).When(x => x.EstimatedUnitPrice.HasValue);
        });

        // Cubiertas: no puede haber dos entradas para la misma posición en un mismo reporte.
        RuleFor(x => x.Tires)
            .Must(tires => tires == null ||
                           tires.Select(t => t.Position).Distinct().Count() == tires.Count)
            .WithMessage("No podés cargar dos veces la misma posición de cubierta.");

        RuleForEach(x => x.Tires).ChildRules(t =>
        {
            t.RuleFor(x => x.Position).IsInEnum();

            // Profundidad de banda: rango físico razonable (0 a 30mm cubre camión/agro).
            t.RuleFor(x => x.InnerDepthMm).InclusiveBetween(0, 30);
            t.RuleFor(x => x.CenterDepthMm).InclusiveBetween(0, 30);
            t.RuleFor(x => x.OuterDepthMm).InclusiveBetween(0, 30);

            t.RuleFor(x => x.Brand).MaximumLength(100);
            t.RuleFor(x => x.Model).MaximumLength(100);
            t.RuleFor(x => x.SizeSpec).MaximumLength(50);
            t.RuleFor(x => x.Notes).MaximumLength(1000);
            t.RuleFor(x => x.InitialTreadDepthMm).InclusiveBetween(0, 30).When(x => x.InitialTreadDepthMm.HasValue);
            t.RuleFor(x => x.ExpectedLifeKm).GreaterThan(0).When(x => x.ExpectedLifeKm.HasValue);
        });

        // Batería: estado válido + voltaje en rango físico (0–24V cubre 12V y 24V).
        When(x => x.Battery is not null, () =>
        {
            RuleFor(x => x.Battery!.Status).IsInEnum();
            RuleFor(x => x.Battery!.Voltage).InclusiveBetween(0, 24).When(x => x.Battery!.Voltage.HasValue);
            RuleFor(x => x.Battery!.RemainingPercentage).InclusiveBetween(0, 100).When(x => x.Battery!.RemainingPercentage.HasValue);
            RuleFor(x => x.Battery!.Brand).MaximumLength(100);
            RuleFor(x => x.Battery!.Notes).MaximumLength(1000);

            // Specs físicas: positivas cuando se informan; borne dentro del enum.
            RuleFor(x => x.Battery!.CapacityAh).GreaterThan(0).When(x => x.Battery!.CapacityAh.HasValue);
            RuleFor(x => x.Battery!.BoxWidthCm).GreaterThan(0).When(x => x.Battery!.BoxWidthCm.HasValue);
            RuleFor(x => x.Battery!.BoxLengthCm).GreaterThan(0).When(x => x.Battery!.BoxLengthCm.HasValue);
            RuleFor(x => x.Battery!.BoxHeightCm).GreaterThan(0).When(x => x.Battery!.BoxHeightCm.HasValue);
            RuleFor(x => x.Battery!.PositiveTerminalSide).IsInEnum().When(x => x.Battery!.PositiveTerminalSide.HasValue);
        });
    }
}
