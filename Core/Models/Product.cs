using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock can't be negative")]
        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Category
        [Required]
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // Seller
        [Required]
        [ForeignKey(nameof(SellerProfile))]
        public int SellerProfileId { get; set; }

        public Sellerprofile? SellerProfile { get; set; }

        // Navigation
        public ICollection<ProductImage> ImagesNames { get; set; } = new HashSet<ProductImage>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new HashSet<WishlistItem>();
        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}