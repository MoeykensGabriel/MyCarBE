using FluentValidation;
using MyCarBE.Application.Common.Validation;

namespace MyCarBE.Application.Features.Fleets.Commands.CreateFleet;

public class CreateFleetCommandValidator : AbstractValidator<CreateFleetCommand>
{
    public CreateFleetCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => (string?)x.TaxId)
            .NotEmpty().WithMessage("El CUIT es obligatorio.")
            .MaximumLength(20).WithMessage("El CUIT no puede superar 20 caracteres.")
            .MustBeValidArgentinaCuit()
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId))
            .OverridePropertyName(nameof(CreateFleetCommand.TaxId));

        RuleFor(x => (string?)x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MustBeValidArgentinaPhone()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .OverridePropertyName(nameof(CreateFleetCommand.Phone));

        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .MaximumLength(150).WithMessage("El email no puede superar 150 caracteres.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");
        });

        When(x => !string.IsNullOrEmpty(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("La dirección no puede superar 300 caracteres.");
        });
    }
}
