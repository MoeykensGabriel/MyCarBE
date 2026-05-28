using FluentValidation;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.VehicleDocuments.Commands.UpdateVehicleDocument;

public class UpdateVehicleDocumentCommandValidator : AbstractValidator<UpdateVehicleDocumentCommand>
{
    public UpdateVehicleDocumentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.ExpiresOn)
            .Must(d => d > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-50))
            .WithMessage("La fecha de vencimiento es inválida.");

        When(x => x.DocumentType == VehicleDocumentType.Other, () =>
            RuleFor(x => x.Notes)
                .NotEmpty()
                .WithMessage("Aclará en 'Notas' qué documento es cuando el tipo es 'Otro'."));

        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.IssuingEntity).MaximumLength(200);
    }
}
