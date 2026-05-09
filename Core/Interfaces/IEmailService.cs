using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        string GenerateVerificationCode();
    }
}
