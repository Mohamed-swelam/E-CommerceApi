using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Order
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public required string ShippingAddress { get; set; }
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFees { get; set; }
        public string UserId { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new List<OrderItemResponseDto>();
    }
}
