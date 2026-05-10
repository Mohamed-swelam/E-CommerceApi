using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Auth
{
    public class ConfirmEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string VerificationCode { get; set; }
    }
}
