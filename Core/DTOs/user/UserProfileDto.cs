using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.user
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ImagePath { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
