using Microsoft.AspNetCore.Http;
namespace Core.DTOs.Category
{
    public class UpdateCategory
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public IFormFile? Icon { get; set; }
    }
}