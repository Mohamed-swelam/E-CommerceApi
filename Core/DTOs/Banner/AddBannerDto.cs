using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Banner
{
    public class AddBannerDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public IFormFile Image { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
