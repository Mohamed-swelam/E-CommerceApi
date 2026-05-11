using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<CartItem> Items { get; set; } = new HashSet<CartItem>();
    }
}
