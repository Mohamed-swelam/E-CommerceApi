using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Account
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public List<string>? Errors { get; set; }
    }
}
