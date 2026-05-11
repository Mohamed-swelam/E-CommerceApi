using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.DTOs.Seller
{
    public class UpdateSellerDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? StoreName { get; set; }

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters")]
        public string? Description { get; set; }

        public IFormFile? Logo { get; set; }
    }
}
