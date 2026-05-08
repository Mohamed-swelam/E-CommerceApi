using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public required string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<CartItem> Items { get; set; } = new HashSet<CartItem>();
    }
}
