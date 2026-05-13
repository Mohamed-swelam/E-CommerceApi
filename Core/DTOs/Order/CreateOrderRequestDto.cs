using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Order
{
    public class CreateOrderRequestDto
    {
        [Required(ErrorMessage = "Shipping address is required.")]
        [MaxLength(250, ErrorMessage = "Shipping address cannot exceed 250 characters." )]
        public string ShippingAddress { get; set; } = string.Empty;

        public string? GuestName { get; set; }

        public string? GuestEmail { get; set; }
    }
}
