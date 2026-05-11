using System.ComponentModel.DataAnnotations.Schema;
namespace Core.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public string ImageName { get; set; }
        public bool IsMain { get; set; }
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}