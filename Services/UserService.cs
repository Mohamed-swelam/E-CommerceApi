using Core.DTOs.GeneralDto;
using Core.DTOs.user;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<GeneralResponse> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return Fail("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserProfileDto
            {
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ImageBase64 = user.Image != null ? Convert.ToBase64String(user.Image) : null,
                Role = roles.FirstOrDefault() ?? "Customer",
                CreatedAt = user.CreatedAt
            });
        }

        public async Task<GeneralResponse> UpdateProfileAsync(UpdateProfileDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return Fail("User not found.");

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Address))
                user.Address = dto.Address;

            if (dto.Image != null)
            {
                using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                user.Image = ms.ToArray();
            }

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return Fail(result.Errors.Select(e => e.Description).ToList());

            return Ok("Profile updated successfully.");
        }
        
        private GeneralResponse Ok(object data) => new() { IsSuccess = true, Data = data };
        private GeneralResponse Fail(object data) => new() { IsSuccess = false, Data = data };
    }
}
