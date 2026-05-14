using Core.DTOs.GeneralDto;
using Core.DTOs.Seller;
using Core.Interfaces.Helpers;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class SellerService : ISellerService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageHelper _imageHelper;


        public SellerService(AppDbContext context, UserManager<ApplicationUser> userManager, IImageHelper imageHelper)
        {
            _context = context;
            _userManager = userManager;
            _imageHelper = imageHelper;
        }

        
        public async Task<GeneralResponse> RegisterSellerAsync(RegisterSellerDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Fail("User not found.");

            var existingSeller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSeller != null)
                return Fail("You are already registered as a seller.");

            var storeExists = await _context.Sellers
                .AnyAsync(s => s.StoreName == dto.StoreName);

            if (storeExists)
                return Fail("Store name already taken. Please choose another.");

            string? logoPath = null;
            if (dto.Logo != null)
            {
                var imageName = await _imageHelper.SaveImageAsync(dto.Logo, "images/sellers");
                logoPath = $"images/sellers/{imageName}";
            }

            var seller = new Sellerprofile
            {
                UserId = userId,
                StoreName = dto.StoreName,
                Description = dto.Description,
                Logo = logoPath,
                IsApproved = false,
                TotalEarnings = 0
            };

            await _context.Sellers.AddAsync(seller);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, "Seller");

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Seller registered successfully. Waiting for admin approval.");
        }

        
        public async Task<GeneralResponse> GetSellerProfileAsync(string userId)
        {
            var seller = await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (seller == null)
                return Fail("Seller profile not found.");

            return Ok(MapToDto(seller));
        }

       
        public async Task<GeneralResponse> GetSellerByIdAsync(int sellerId)
        {
            var seller = await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null)
                return Fail("Seller not found.");

            return Ok(MapToDto(seller));
        }

        
        public async Task<GeneralResponse> GetAllSellersAsync()
        {
            var sellers = await _context.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .ToListAsync();

            var result = sellers.Select(MapToDto).ToList();

            return Ok(result);
        }

        public async Task<GeneralResponse> UpdateSellerProfileAsync(UpdateSellerDto dto, string userId)
        {
            var seller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (seller == null)
                return Fail("Seller profile not found.");

            if (!seller.IsApproved)
                return Fail("Your seller account is not approved yet.");

            if (!string.IsNullOrEmpty(dto.StoreName) && dto.StoreName != seller.StoreName)
            {
                var storeExists = await _context.Sellers
                    .AnyAsync(s => s.StoreName == dto.StoreName);

                if (storeExists)
                    return Fail("Store name already taken.");

                seller.StoreName = dto.StoreName;
            }

            if (!string.IsNullOrEmpty(dto.Description))
                seller.Description = dto.Description;

            if (dto.Logo != null)
            {
                if (!string.IsNullOrEmpty(seller.Logo))
                    _imageHelper.DeleteImage(seller.Logo, "images/sellers");

                var imageName = await _imageHelper.SaveImageAsync(dto.Logo, "images/sellers");
                seller.Logo = $"images/sellers/{imageName}";
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
                user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Seller profile updated successfully.");
        }

        public async Task<GeneralResponse> DeleteSellerAsync(string userId)
        {
            var seller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (seller == null)
                return Fail("Seller profile not found.");

            if (!string.IsNullOrEmpty(seller.Logo))
                _imageHelper.DeleteImage(seller.Logo, "images/sellers");

            _context.Sellers.Remove(seller);

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, "Customer");
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok("Seller account removed successfully.");
        }

        private static SellerResponseDto MapToDto(Sellerprofile seller) => new()
        {
            Id = seller.Id,
            UserId = seller.UserId,
            FullName = seller.User?.FullName ?? string.Empty,
            Email = seller.User?.Email ?? string.Empty,
            StoreName = seller.StoreName,
            Description = seller.Description,
            LogoBase64 = seller.Logo,
            IsApproved = seller.IsApproved,
            TotalEarnings = seller.TotalEarnings,
            TotalProducts = seller.Products?.Count ?? 0
        };

        private GeneralResponse Ok(object data) => new() { IsSuccess = true, Data = data };
        private GeneralResponse Fail(object data) => new() { IsSuccess = false, Data = data };
    }
}
