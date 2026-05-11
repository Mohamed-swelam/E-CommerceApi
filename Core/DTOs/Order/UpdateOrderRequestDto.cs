using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Order
{
    public class UpdateOrderRequestDto
    {
        [Required]
        [MaxLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        public OrderStatus Status { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
