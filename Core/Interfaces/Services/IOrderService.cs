using Core.DTOs.GeneralDto;
using Core.DTOs.Order;

namespace Core.Interfaces.Services
{
    public interface IOrderService
    {
        Task<GeneralResponse> GetAllOrdersAsync(string? userId, string? guestId);

        Task<GeneralResponse> GetOrderByIdAsync(int id, string? userId, string? guestId);

        Task<GeneralResponse> CreateOrderAsync(CreateOrderRequestDto dto, string? userId, string? guestId);

        Task<GeneralResponse> CancelOrderAsync(int id, string? userId, string? guestId);
        Task<GeneralResponse> GetOrderStatusAsync(int id, string? userId, string? guestId);
    }
}
