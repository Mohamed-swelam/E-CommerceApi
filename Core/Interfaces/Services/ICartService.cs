using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;

namespace Core.Interfaces.Services
{

    public interface ICartService
    {
        Task<CartResponseDto?> GetUserCartAsync(string? userId, string? guestId);

        Task<GeneralResponse> AddToCartAsync(AddToCartDto dto, string? userId, string? guestId);

        Task<GeneralResponse> UpdateCartItemAsync(UpdateCartItemDto dto, string? userId, string? guestId);

        Task<GeneralResponse> RemoveCartItemAsync(int productId, string? userId, string? guestId);

        Task<GeneralResponse> ClearCartAsync(string? userId, string? guestId);
        Task MergeGuestCartAsync(string userId,string guestId);
    }

}
