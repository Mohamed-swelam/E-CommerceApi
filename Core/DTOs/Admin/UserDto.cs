using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Admin
{
    public class UserDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Role { get; set; }  
        public bool IsBlocked { get; set; }  = false;     
        public bool IsSellerApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
