using Core.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required(ErrorMessage = "Name is Required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string FullName { get; set; }

        public string? ImagePath { get; set; }
        public string Role { get; set; } = "user";  
        public bool IsDeleted { get; set; } = false; 
        public bool IsBlocked { get; set; } = false;  
        public bool IsSellerApproved { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100, ErrorMessage = "Address must be at most 100 characters")]
        public string? Address { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public Sellerprofile? Seller { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<WishlistItem> Wishlists { get; set; }
        public Cart Cart { get; set; }
    }
}
