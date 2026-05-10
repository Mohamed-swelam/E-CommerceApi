using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var smtpClient = new SmtpClient(emailSettings["Host"])
            {
                Port = int.Parse(emailSettings["Port"]!),
                Credentials = new NetworkCredential(emailSettings["Email"], emailSettings["Password"]),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(emailSettings["Email"]!, emailSettings["DisplayName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await smtpClient.SendMailAsync(message);
        }

        public string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
