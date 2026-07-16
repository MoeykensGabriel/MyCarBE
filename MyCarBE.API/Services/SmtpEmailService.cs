using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

/// <summary>
/// Envío de emails vía SMTP con MailKit (reemplaza a System.Net.Mail.SmtpClient,
/// que Microsoft desaconseja y NO soporta SSL implícito en el puerto 465 — que es
/// justamente el que usa Ferozo, el proveedor de correo del taller).
///
/// Config EmailSettings (por env vars EmailSettings__* en Railway):
///   Host / Port / User / Password / From [/ FromName]
/// Ferozo: Host=dtc027.ferozo.com, Port=465 (SSL). SecureSocketOptions.Auto elige
/// SSL implícito para 465 y STARTTLS para 587 — funciona con cualquiera de los dos.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody,
        byte[]? attachment = null, string? attachmentName = null,
        CancellationToken cancellationToken = default)
    {
        var section  = _config.GetSection("EmailSettings");
        var host     = section["Host"]!;
        var port     = int.Parse(section["Port"]!);
        var user     = section["User"]!;
        var pass     = section["Password"]!;
        var from     = section["From"]!;
        var fromName = section["FromName"] ?? from;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachment is not null && attachmentName is not null)
            bodyBuilder.Attachments.Add(attachmentName, attachment, ContentType.Parse("application/pdf"));
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        // Auto: 465 → SSL implícito (SslOnConnect); 587 → STARTTLS.
        await client.ConnectAsync(host, port, SecureSocketOptions.Auto, cancellationToken);
        await client.AuthenticateAsync(user, pass, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email enviado a {To} — {Subject}", to, subject);
    }
}
