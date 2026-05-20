using FluentValidation;

namespace MyCarBE.Application.Features.WorkOrders.Commands.UploadWorkOrderPhoto;

/// <summary>
/// Validator del upload de fotos del WorkOrder. Es crítico porque el archivo
/// viaja al storage sin más controles. Sin esto, alguien podría subir un
/// ejecutable, un PDF disfrazado, o un archivo enorme que llene el disco.
/// </summary>
public class UploadWorkOrderPhotoCommandValidator : AbstractValidator<UploadWorkOrderPhotoCommand>
{
    // Solo aceptamos las extensiones de imagen comunes y modernas.
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif",
    };

    // 10 MB. Suficiente para fotos modernas de teléfono, evita PDFs de 50 MB
    // o archivos malformados de varios GB que romperían el disco del server.
    private const long MaxFileSizeBytes = 10L * 1024 * 1024;

    public UploadWorkOrderPhotoCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("El id de la orden es obligatorio.");

        RuleFor(x => x.PhotoType)
            .IsInEnum().WithMessage("El tipo de foto no es válido.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("El archivo es obligatorio.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("El nombre del archivo es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre del archivo no puede superar 255 caracteres.")
            .Must(HaveAllowedExtension)
                .WithMessage("Solo se aceptan imágenes: jpg, jpeg, png, webp, heic, heif.");

        // Validamos el tamaño solo si el stream lo expone (los IFormFile streams sí lo hacen).
        // Streams no-seekable devolverían 0 / lanzarían — `When` nos protege.
        When(x => x.FileStream != null && x.FileStream.CanSeek, () =>
        {
            RuleFor(x => x.FileStream.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage($"El archivo no puede superar {MaxFileSizeBytes / (1024 * 1024)} MB.")
                .GreaterThan(0)
                .WithMessage("El archivo está vacío.");
        });

        When(x => !string.IsNullOrEmpty(x.Caption), () =>
        {
            RuleFor(x => x.Caption)
                .MaximumLength(500)
                .WithMessage("La descripción no puede superar 500 caracteres.");
        });
    }

    private static bool HaveAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(ext) && AllowedExtensions.Contains(ext);
    }
}
