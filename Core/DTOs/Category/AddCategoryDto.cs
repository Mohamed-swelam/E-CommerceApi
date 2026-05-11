using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace Core.DTOs.Category
{
    public class AddCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public IFormFile Icon { get; set; }
    }
}