using System.ComponentModel.DataAnnotations;
namespace Core.DTOs.Product
{
    public class UpdateProductDto
    {
        public int ProductId { get; set; }
        [MaxLength(100)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative number")]
        public int? StockQuantity { get; set; }
        public int? CategoryId { get; set; }
    }
}