using Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        [Required]
        [MaxLength(250)]
        public required string ShippingAddress { get; set; }

        
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new HashSet<OrderItem>();

        public Payment? Payment { get; set; }

        public Shipping? Shipping { get; set; }


        public string? GuestName { get; set; }

        public string? GuestEmail{get; set;}

        public string? GuestId { get; set; }
    }
}
