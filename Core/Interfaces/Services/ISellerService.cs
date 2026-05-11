using Core.DTOs.GeneralDto;
using Core.DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces.Services
{
    public interface ISellerService
    {
        Task<GeneralResponse> RegisterSellerAsync(RegisterSellerDto dto, string userId);
        Task<GeneralResponse> GetSellerProfileAsync(string userId);
        Task<GeneralResponse> GetSellerByIdAsync(int sellerId);
        Task<GeneralResponse> GetAllSellersAsync();
        Task<GeneralResponse> UpdateSellerProfileAsync(UpdateSellerDto dto, string userId);
        Task<GeneralResponse> DeleteSellerAsync(string userId);
    }
}
