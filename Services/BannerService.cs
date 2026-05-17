using Core.DTOs.Banner;
using Core.DTOs.GeneralDto;
using Core.Interfaces.Helpers;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageHelper _imageHelper;

        public BannerService(IBannerRepository bannerRepository, ICategoryRepository categoryRepository, IImageHelper imageHelper)
        {
            _bannerRepository = bannerRepository;
            _categoryRepository = categoryRepository;
            _imageHelper = imageHelper;
        }

        public async Task<GeneralResponse> AddBannerAsync(AddBannerDto addBannerDto)
        {
            var category = await _categoryRepository.GetByIdAsync(addBannerDto.CategoryId);
            if (category == null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "Category not found" };
            }

            string imageName = null;
            if (addBannerDto.Image != null)
            {
                imageName = await _imageHelper.SaveImageAsync(addBannerDto.Image, "Banners");
            }

            var banner = new Banner
            {
                Title = addBannerDto.Title,
                CategoryId = addBannerDto.CategoryId,
                Image = imageName,
                IsActive = addBannerDto.IsActive
            };

            await _bannerRepository.AddAsync(banner);
            await _bannerRepository.SaveChangesAsync();

            return new GeneralResponse { IsSuccess = true, Data = "Banner added successfully" };
        }

        public async Task<GeneralResponse> DeleteBannerAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "Banner not found" };
            }

            _bannerRepository.Delete(banner);
            await _bannerRepository.SaveChangesAsync();

            return new GeneralResponse { IsSuccess = true, Data = "Banner deleted successfully" };
        }

        public async Task<GeneralResponse> GetActiveBannersAsync()
        {
            var banners = await _bannerRepository.GetActiveBannersAsync();
            var dtos = banners.Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.Name,
                Image = b.Image,
                IsActive = b.IsActive
            });

            return new GeneralResponse { IsSuccess = true, Data = dtos };
        }

        public async Task<GeneralResponse> GetAllBannersAsync()
        {
            var banners = await _bannerRepository.GetAll().ToListAsync();
            // Since GetAllAsync doesn't include Category by default (unless eager loaded in repo), we might just map what we have or get it via another method.
            // But we can just use GetActiveBannersAsync approach or load it if necessary.
            // Let's assume standard GetAllAsync is fine without category name for now, or we can fetch it.
            var dtos = banners.Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                CategoryId = b.CategoryId,
                Image = b.Image,
                IsActive = b.IsActive
            });

            return new GeneralResponse { IsSuccess = true, Data = dtos };
        }

        public async Task<GeneralResponse> GetBannerByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "Banner not found" };
            }

            var dto = new BannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                CategoryId = banner.CategoryId,
                Image = banner.Image,
                IsActive = banner.IsActive
            };

            return new GeneralResponse { IsSuccess = true, Data = dto };
        }

        public async Task<GeneralResponse> UpdateBannerAsync(int id, UpdateBannerDto updateBannerDto)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "Banner not found" };
            }

            var category = await _categoryRepository.GetByIdAsync(updateBannerDto.CategoryId);
            if (category == null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "Category not found" };
            }

            banner.Title = updateBannerDto.Title;
            banner.CategoryId = updateBannerDto.CategoryId;
            banner.IsActive = updateBannerDto.IsActive;

            if (updateBannerDto.Image != null)
            {
                banner.Image = await _imageHelper.SaveImageAsync(updateBannerDto.Image, "Banners");
            }

            _bannerRepository.Update(banner);
            await _bannerRepository.SaveChangesAsync();

            return new GeneralResponse { IsSuccess = true, Data = "Banner updated successfully" };
        }
    }
}
