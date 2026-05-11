using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Order
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
