﻿using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailServiceOptions _emailNotifierOptions;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailServiceOptions> emailNotifierOptions)
        {
            _logger = logger;
            _emailNotifierOptions = emailNotifierOptions.Value;
        }

        public async Task SendEmailAsync(string subject, string body, string[] recipients)
        {
            _logger.LogInformation("Sending email to {recipients}", string.Join(", ", recipients));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(string.Empty, _emailNotifierOptions.SmtpUserName));

            foreach (string recipient in recipients)
            {
                message.Bcc.Add(new MailboxAddress(string.Empty, recipient));
            }

            message.Subject = subject;

            message.Body = new TextPart
            {
                Text = body
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(_emailNotifierOptions.SmtpServer, _emailNotifierOptions.SmtpPort, _emailNotifierOptions.UseSsl);

            await client.AuthenticateAsync(_emailNotifierOptions.SmtpUserName, _emailNotifierOptions.SmtpPassword);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}
