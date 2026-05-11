namespace Core.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        //Navigation
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}