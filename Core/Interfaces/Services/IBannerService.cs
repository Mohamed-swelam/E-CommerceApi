using Core.DTOs.Banner;
using Core.DTOs.GeneralDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IBannerService
    {
        Task<GeneralResponse> GetAllBannersAsync();
        Task<GeneralResponse> GetActiveBannersAsync();
        Task<GeneralResponse> GetBannerByIdAsync(int id);
        Task<GeneralResponse> AddBannerAsync(AddBannerDto addBannerDto);
        Task<GeneralResponse> UpdateBannerAsync(int id, UpdateBannerDto updateBannerDto);
        Task<GeneralResponse> DeleteBannerAsync(int id);
    }
}
