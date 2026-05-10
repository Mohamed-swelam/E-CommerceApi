using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;

namespace Core.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartResponseDto?> GetUserCartAsync(string userId);
        Task<GeneralResponse> AddToCartAsync(AddToCartDto dto, string userId);
        Task<GeneralResponse> UpdateCartItemAsync(UpdateCartItemDto dto, string userId);
        Task<GeneralResponse> RemoveCartItemAsync(int productId, string userId);
        Task<GeneralResponse> ClearCartAsync(string userId);
    }
}
