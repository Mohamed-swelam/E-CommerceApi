namespace Core.DTOs.Banner
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
    }
}
