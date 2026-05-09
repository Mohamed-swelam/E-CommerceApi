using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Admin
{
    public class ChangeRoleDto
    {
        [Required]
        [RegularExpression("Admin|Seller|Customer", ErrorMessage = "Role must be Admin, Seller, or Customer")]
        public string Role { get; set; }
    }
}
