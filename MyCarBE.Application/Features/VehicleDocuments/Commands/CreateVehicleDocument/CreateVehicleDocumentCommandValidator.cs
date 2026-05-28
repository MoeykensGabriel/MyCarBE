using FluentValidation;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.CreateVehicleDocument;

public class CreateVehicleDocumentCommandValidator : AbstractValidator<CreateVehicleDocumentCommand>
{
    public CreateVehicleDocumentCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.ExpiresOn)
            .Must(d => d > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-50))
            .WithMessage("La fecha de vencimiento es inválida.");

        // Si el tipo es Other, obligamos a aclarar en Notes qué documento es.
        When(x => x.DocumentType == VehicleDocumentType.Other, () =>
            RuleFor(x => x.Notes)
                .NotEmpty()
                .WithMessage("Aclará en 'Notas' qué documento es cuando el tipo es 'Otro'."));

        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.IssuingEntity).MaximumLength(200);
    }
}
