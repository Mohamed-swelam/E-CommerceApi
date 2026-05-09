using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Auth
{
    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
