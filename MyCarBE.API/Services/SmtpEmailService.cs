using System.Net;
using System.Net.Mail;
using MyCarBE.Application.Common.Interfaces;

namespace MyCarBE.API.Services;

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
        var section = _config.GetSection("EmailSettings");
        var host    = section["Host"]!;
        var port    = int.Parse(section["Port"]!);
        var user    = section["User"]!;
        var pass    = section["Password"]!;
        var from    = section["From"]!;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl   = true
        };

        using var message = new MailMessage(from, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        if (attachment is not null && attachmentName is not null)
        {
            var stream = new MemoryStream(attachment);
            message.Attachments.Add(new Attachment(stream, attachmentName, "application/pdf"));
        }

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email enviado a {To} — {Subject}", to, subject);
    }
}
