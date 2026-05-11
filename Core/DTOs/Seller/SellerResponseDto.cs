using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTOs.Seller
{
    public class SellerResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string StoreName { get; set; }
        public string Description { get; set; }
        public string? LogoBase64 { get; set; }
        public bool IsApproved { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalProducts { get; set; }
    }
}
