using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Seller
{
    public class RegisterSellerDto
    {
        [Required(ErrorMessage = "Store name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string StoreName { get; set; }

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters")]
        public string Description { get; set; }

        public IFormFile? Logo { get; set; }
    }
}
