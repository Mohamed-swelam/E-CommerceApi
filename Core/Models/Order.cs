using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        
        
        public required string ShippingAddress { get; set; }

        [Required]
        public required string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();

        public Payment? Payment { get; set; }

        public Shipping? Shipping { get; set; }
    }
}
