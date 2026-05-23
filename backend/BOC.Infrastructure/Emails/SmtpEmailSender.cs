using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using BOC.Application.Common.Interfaces;

namespace BOC.Infrastructure.Emails;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger        = logger;
    }

    /// <inheritdoc/>
    public Task SendEmailAsync(string toEmail, string subject, string body)
        => SendEmailAsync(toEmail, subject, body, CancellationToken.None);

    /// <inheritdoc/>
    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        var fromEmail   = _configuration["Smtp:FromEmail"]    ?? "hr-planing@boc.oil.gov.iq";
        var displayName = _configuration["Smtp:FromDisplayName"] ?? "BOC HR Research Planning";
        var host        = _configuration["Smtp:Host"]         ?? "mail.boc.oil.gov.iq";
        var port        = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 465;
        var password    = _configuration["Smtp:Password"];

        _logger.LogInformation(
            "Attempting to send email to {ToEmail} with subject '{Subject}' via {Host}:{Port}...",
            toEmail, subject, host, port);

        if (string.IsNullOrEmpty(password))
        {
            _logger.LogWarning(
                "SMTP Password is not configured. Logging email content (Mock Delivery):\nTo: {ToEmail}\nSubject: {Subject}\nBody:\n{Body}",
                toEmail, subject, body);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(displayName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body   = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var secureOption = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(host, port, secureOption, cancellationToken);
            await client.AuthenticateAsync(fromEmail, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP.", toEmail);
            throw;
        }
    }
}
