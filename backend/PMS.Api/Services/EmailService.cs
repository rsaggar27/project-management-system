using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PMS.Api.Models;

namespace PMS.Api.Services;

public class EmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body)
    {
        if (!_options.Enabled)
            return;

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            Credentials = new NetworkCredential(
                _options.Username,
                _options.Password),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }
}
