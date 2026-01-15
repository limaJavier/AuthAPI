using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthAPI.Infrastructure.Services;

public class GmailSender(IOptions<EmailSettings> options, ILogger<GmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;
    private readonly ILogger<GmailSender> _logger = logger;

    public async Task<Result> SendAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect to the SMTP server
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
            );

            // Authenticate
            await client.AuthenticateAsync(_settings.Username, _settings.Password);

            // Send the email
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Error.Unexpected(e.Message);
        }
    }
}
