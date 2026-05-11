using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Shipping
    {
        [Key]
        public int ShippingId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string TrackingNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Carrier { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        [Required]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        public Order? Order { get; set; }
    }
}
