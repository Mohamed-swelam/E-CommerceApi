namespace Core.DTOs.Product
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; }
        public string StoreName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ImagesNames { get; set; } = new List<string>();
    }
}