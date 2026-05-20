namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Envío de emails transaccionales.
/// Implementado en API con SMTP; intercambiable por SendGrid en producción.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody,
        byte[]? attachment = null, string? attachmentName = null,
        CancellationToken cancellationToken = default);
}
