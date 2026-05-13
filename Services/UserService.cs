using Core.DTOs.GeneralDto;
using Core.DTOs.user;
using Core.Interfaces.Helpers;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Identity;


namespace Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageHelper _imageHelper;
        public UserService(UserManager<ApplicationUser> userManager, IImageHelper imageHelper)
        {
            _userManager = userManager;
            _imageHelper = imageHelper;
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
                ImagePath = user.ImagePath, 
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
                if (!string.IsNullOrEmpty(user.ImagePath))
                    _imageHelper.DeleteImage(user.ImagePath, "images/users");

                var imageName = await _imageHelper.SaveImageAsync(dto.Image, "images/users");
                user.ImagePath = $"images/users/{imageName}";
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
