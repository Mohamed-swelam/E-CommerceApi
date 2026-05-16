using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Admin
{
    public class ChangeRoleDto
    {
        [Required]
        [RegularExpression("Admin|Seller|user", ErrorMessage = "Role must be Admin, Seller, or user")]
        public string Role { get; set; }
    }
}
