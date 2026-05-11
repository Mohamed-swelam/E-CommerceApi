using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;
using Core.DTOs.Product;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces.Services
{
    public interface IProductService
    {
        Task<GeneralResponse> GetProductByIdAsync(int productId);
        Task<GeneralResponse> GetAllProductsAsync();
        Task<GeneralResponse> AddProductImageAsync(int productId, IFormFile image);
        Task<GeneralResponse> AddProductAsync(AddProductDto dto, string userId);
        Task<GeneralResponse> UpdateProductAsync(UpdateProductDto dto, string userId);
        Task<GeneralResponse> RemoveProductAsync(int productId, string userId);
    }
}
