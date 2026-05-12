using Core.DTOs.GeneralDto;
using Core.DTOs.Order;

namespace Core.Interfaces.Services
{
    public interface IOrderService
    {
        Task<GeneralResponse> GetAllOrdersAsync(string userId);

        Task<GeneralResponse> GetOrderByIdAsync(int id, string userId);

        Task<GeneralResponse> CreateOrderAsync(CreateOrderRequestDto dto,string userId);

        Task<GeneralResponse> CancelOrderAsync(int id,string userId);
        Task<GeneralResponse> GetOrderStatusAsync(int id, string userId);
    }
}
