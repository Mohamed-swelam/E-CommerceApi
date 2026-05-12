using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class Sellerprofile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public byte[] Logo { get; set; }

        [Required(ErrorMessage = "Store name is Required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Store name must be between 2 and 100 characters")]
        public string StoreName { get; set; }

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters")]
        public string Description { get; set; }

        public bool IsApproved { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Earnings must be Positive")]
        public decimal TotalEarnings { get; set; } = 0;

        public ICollection<Product> Products { get; set; } = new HashSet<Product>();

    }
}
