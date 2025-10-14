using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;

namespace TaskFlow.Infrastructure.Services
{
    public class EmailSender : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SendEmailAsync(string recipientMail, string subject, string htmlBody)
        {
            try
            {

                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];

                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("TaskFlowAPP", fromEmail));
                email.To.Add(new MailboxAddress("", recipientMail));
                email.Subject = subject;

                //Email body(HTML or plain text)
                email.Body = new TextPart("html")
                {
                    Text = htmlBody
                };

                // Configure the SMTP client
                using (var smtpClient = new SmtpClient())
                {
                    // Connect to the SMTP server
                    await smtpClient.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.SslOnConnect);

                    // Authenticate with the SMTP server
                    await smtpClient.AuthenticateAsync(username, password);

                    // Send the email
                    await smtpClient.SendAsync(email);
                    _logger.LogInformation($"{subject} Email successfully sent to {recipientMail} ");
                    // Disconnect from the SMTP server
                    await smtpClient.DisconnectAsync(true);
                }

                return "Email Sent successfully";
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"An error {ex.Message} occured while trying to send email {subject} to {recipientMail}");
                return string.Empty;
            }
        }
    }
}
