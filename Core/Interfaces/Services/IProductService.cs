using Core.DTOs.GeneralDto;
using Core.DTOs.Product;
using Microsoft.AspNetCore.Http;
namespace Core.Interfaces.Services
{
    public interface IProductService
    {
        Task<GeneralResponse> GetProductByIdAsync(int productId);
        Task<GeneralResponse> GetAllProductsAsync(ProductFilterDto filter);
        Task<GeneralResponse> AddProductImageAsync(int productId, IFormFile image, string userId);
        Task<GeneralResponse> AddProductAsync(AddProductDto dto, string userId);
        Task<GeneralResponse> UpdateProductAsync(UpdateProductDto dto, string userId);
        Task<GeneralResponse> RemoveProductAsync(int productId, string userId);
        Task<GeneralResponse> GetProductsBySellerAsync(string sellerProfileId);
    }
}