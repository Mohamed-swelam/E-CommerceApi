using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; }

        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Seller
        public int SellerId { get; set; }
        public Sellerprofile? Seller { get; set; }

        // Navigation
        public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();

        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}
