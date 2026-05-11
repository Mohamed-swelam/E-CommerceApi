using Core.DTOs.Admin;
using Core.Enums;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 

    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new { Success = true, Data = users });
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound(new { Success = false, Message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Success = true,
                Data = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    Role = roles.FirstOrDefault()
                }
            });
        }

        [HttpPut("block-user/{id}")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found" });

            if (user.IsDeleted)
                return BadRequest(new { Success = false, Message = "User is already blocked" });

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Success = true, Message = "User blocked successfully" });
        }

        [HttpPut("unblock-user/{id}")]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found" });

            if (!user.IsDeleted)
                return BadRequest(new { Success = false, Message = "User is not blocked" });

            user.IsDeleted = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Success = true, Message = "User unblocked successfully" });
        }

        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found" });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { Success = true, Message = "User deleted successfully" });
        }

        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found" });

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { Success = true, Message = $"Role changed to {dto.Role}" });
        }

        [HttpPut("approve-seller/{sellerId}")]
        public async Task<IActionResult> ApproveSeller(int sellerId)
        {
            var seller = await _context.Sellers.FindAsync(sellerId);

            if (seller == null)
                return NotFound(new { Success = false, Message = "Seller not found" });

            if (seller.IsApproved)
                return BadRequest(new { Success = false, Message = "Seller already approved" });

            seller.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Seller approved successfully" });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalUsers = await _userManager.Users.CountAsync();

            var totalOrders = await _context.Orders.CountAsync();

            var totalProducts = await _context.Products.CountAsync();

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.OrderId,
                    CustomerName = o.User.FullName,
                    o.TotalAmount,
                    o.Status,
                    o.OrderDate
                })
                .ToListAsync();

            return Ok(new
            {
                Success = true,
                Data = new
                {
                    TotalUsers = totalUsers,
                    TotalOrders = totalOrders,
                    TotalProducts = totalProducts,
                    TotalRevenue = totalRevenue,
                    RecentOrders = recentOrders
                }
            });
        }
    }
}
