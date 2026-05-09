using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Auth
{
    public class TokenRequestDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
