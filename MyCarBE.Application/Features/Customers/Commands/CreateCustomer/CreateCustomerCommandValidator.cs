using FluentValidation;
using MyCarBE.Application.Common.Validation;

namespace MyCarBE.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("El tipo de documento no es válido.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(50).WithMessage("El número de documento no puede superar 50 caracteres.");

        // Validación según el tipo de documento (DNI, CUIT/CUIL con checksum, Pasaporte)
        RuleFor(x => (string?)x.DocumentNumber)
            .MustMatchDocumentType(x => x.DocumentType)
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber))
            .OverridePropertyName(nameof(CreateCustomerCommand.DocumentNumber));

        RuleFor(x => (string?)x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MustBeValidArgentinaPhone()
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .OverridePropertyName(nameof(CreateCustomerCommand.Phone));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio para crear el acceso al portal.")
            .MaximumLength(150).WithMessage("El email no puede superar 150 caracteres.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");
    }
}
