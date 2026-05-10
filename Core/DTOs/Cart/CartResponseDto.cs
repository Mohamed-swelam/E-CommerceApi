namespace Core.DTOs.Cart
{
    public class CartResponseDto
    {
        public int CartId { get; set; }

        public List<CartItemResponseDto> Items { get; set; } = new();

        public decimal TotalPrice => Items.Sum(i => i.Total);
    }
}
