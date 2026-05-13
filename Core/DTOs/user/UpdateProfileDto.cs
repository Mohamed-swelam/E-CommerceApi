using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.user
{
    public class UpdateProfileDto
    {
        [StringLength(50, MinimumLength = 3 , ErrorMessage = "FullName must be between 3 and 50 characters.")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Address must be at most 100 characters.")]
        public string? Address { get; set; }

        public IFormFile? Image { get; set; }
    }
}
