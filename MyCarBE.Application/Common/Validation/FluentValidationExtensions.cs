using FluentValidation;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Common.Validation;

/// <summary>
/// Reglas reutilizables de FluentValidation para identificadores argentinos.
/// Mantienen los mensajes centralizados en español.
/// </summary>
public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> MustBeValidArgentinaDni<T>(
        this IRuleBuilder<T, string?> rule)
        => rule
            .Must(ArgentinaIdentifiers.IsValidDni)
            .WithMessage("El DNI debe tener 7 u 8 dígitos.");

    public static IRuleBuilderOptions<T, string?> MustBeValidArgentinaCuit<T>(
        this IRuleBuilder<T, string?> rule)
        => rule
            .Must(ArgentinaIdentifiers.IsValidCuitOrCuil)
            .WithMessage("El CUIT debe tener 11 dígitos. Ej: 30-12345678-9.");

    public static IRuleBuilderOptions<T, string?> MustBeValidArgentinaPhone<T>(
        this IRuleBuilder<T, string?> rule)
        => rule
            .Must(ArgentinaIdentifiers.IsValidPhone)
            .WithMessage("El teléfono no tiene un formato válido. Debe tener entre 8 y 14 dígitos. Ej: +54 9 11 1234 5678.");

    public static IRuleBuilderOptions<T, string?> MustBeValidPassport<T>(
        this IRuleBuilder<T, string?> rule)
        => rule
            .Must(ArgentinaIdentifiers.IsValidPassport)
            .WithMessage("El pasaporte debe ser alfanumérico, entre 5 y 15 caracteres.");

    /// <summary>
    /// Valida un DocumentNumber según el DocumentType del request.
    /// DNI → 7-8 dígitos. CUIT/CUIL → checksum AFIP. Pasaporte → alfanumérico 5-15.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustMatchDocumentType<T>(
        this IRuleBuilder<T, string?> rule,
        Func<T, DocumentType> getType)
        => rule
            .Must((root, value) => getType(root) switch
            {
                DocumentType.DNI      => ArgentinaIdentifiers.IsValidDni(value),
                DocumentType.CUIT     => ArgentinaIdentifiers.IsValidCuitOrCuil(value),
                DocumentType.CUIL     => ArgentinaIdentifiers.IsValidCuitOrCuil(value),
                DocumentType.Passport => ArgentinaIdentifiers.IsValidPassport(value),
                _                     => false,
            })
            .WithMessage((root, _) => getType(root) switch
            {
                DocumentType.DNI      => "El DNI debe tener 7 u 8 dígitos.",
                DocumentType.CUIT     => "El CUIT debe tener 11 dígitos. Ej: 30-12345678-9.",
                DocumentType.CUIL     => "El CUIL debe tener 11 dígitos. Ej: 27-12345678-3.",
                DocumentType.Passport => "El pasaporte debe ser alfanumérico, entre 5 y 15 caracteres.",
                _                     => "Tipo de documento no soportado.",
            });
}
