using Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        // Stripe Payment Intent Id
        [Required]
        [MaxLength(200)]
        public required string TransactionId { get; set; }

        // Stripe Session / Intent Secret
        [MaxLength(500)]
        public string? ClientSecret { get; set; }

        public PaymentStatus Status { get; set; }

        [Required]
        [MaxLength(50)]
        public required string PaymentMethod { get; set; }

        public DateTime? PaidAt { get; set; }

        [Required]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        public Order? Order { get; set; }
    }
}
