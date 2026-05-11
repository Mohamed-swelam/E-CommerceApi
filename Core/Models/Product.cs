using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
namespace Core.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        //Category
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        // Seller
        [ForeignKey(nameof(Sellerprofile))]
        public int SellerProfileId { get; set; }
        public Sellerprofile? Seller { get; set; }
        // Navigation
        public ICollection<ProductImage> ImagesNames { get; set; } = new HashSet<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}