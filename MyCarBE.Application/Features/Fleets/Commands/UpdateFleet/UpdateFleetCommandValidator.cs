using FluentValidation;

namespace MyCarBE.Application.Features.Fleets.Commands.UpdateFleet;

public class UpdateFleetCommandValidator : AbstractValidator<UpdateFleetCommand>
{
    public UpdateFleetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id de la flota es obligatorio.");

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("El CUIT/RUC es obligatorio.")
            .MaximumLength(20).WithMessage("El CUIT/RUC no puede superar 20 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede superar 30 caracteres.");

        When(x => x.Email is not null, () =>
        {
            RuleFor(x => x.Email)
                .MaximumLength(150).WithMessage("El email no puede superar 150 caracteres.")
                .EmailAddress().WithMessage("El email no tiene un formato válido.");
        });

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("La dirección no puede superar 300 caracteres.");
        });
    }
}
