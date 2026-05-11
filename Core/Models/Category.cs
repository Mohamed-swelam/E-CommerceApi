using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        //Navigation
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
