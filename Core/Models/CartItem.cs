using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

     
        [Required]
        [ForeignKey(nameof(Cart))]
        public int CartId { get; set; }
        public Cart? Cart { get; set; }


        [Required]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
