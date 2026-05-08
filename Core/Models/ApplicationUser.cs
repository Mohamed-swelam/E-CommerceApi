using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(30)]
        public required string FirstName { get; set; }
        [MaxLength(30)]
        public required string LastName { get; set; }
        [MaxLength(100)]
        public string? Address { get; set; }
        public int Age { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();

        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        public ICollection<WishlistItem> WishlistItems { get; set; } = new HashSet<WishlistItem>();
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();

    }
}
