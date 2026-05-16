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
            var dbUsers = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var userDtos = new List<UserDto>();

            foreach (var u in dbUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var userRole = roles.FirstOrDefault() ?? u.Role;

                if (string.IsNullOrEmpty(userRole)) userRole = "Customer";

                var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == u.Id);
                bool isApproved = seller?.IsApproved ?? false;

                userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt,
                    Role = userRole,
                    IsBlocked = u.IsBlocked,
                    IsSellerApproved = isApproved 
                });
            }

            return Ok(new { Success = true, Data = userDtos });
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted)
                return NotFound(new { Success = false, Message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? user.Role;
            if (string.IsNullOrEmpty(userRole)) userRole = "Customer";

            var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.UserId == user.Id);

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
                    Role = userRole,
                    IsDeleted = user.IsDeleted,
                    IsBlocked = user.IsBlocked,
                    IsSellerApproved = seller?.IsApproved ?? false, 
                    CreatedAt = user.CreatedAt,
                }
            });
        }

        [HttpPut("block-user/{id}")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.IsDeleted) 
                return NotFound(new { Success = false, Message = "User not found" });

            if (user.IsBlocked)  
                return BadRequest(new { Success = false, Message = "User is already blocked" });

            user.IsBlocked = true;
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

            if (user == null || user.IsDeleted) 
                return NotFound(new { Success = false, Message = "User not found" });

            if (!user.IsBlocked) 
                return BadRequest(new { Success = false, Message = "User is not blocked" });

            user.IsBlocked = false;
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
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == id)
                return BadRequest(new { Success = false, Message = "You cannot change your own role." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Success = false, Message = "User not found" });

            string targetRole = dto.Role?.ToLower() switch
            {
                "admin" => "Admin",
                "seller" => "Seller",
                _ => "Customer"
            };

            user.Role = targetRole;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, targetRole);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { Success = true, Message = $"Role changed to {targetRole} successfully." });
        }

        [HttpPut("approve-seller/{sellerId}")]
        public async Task<IActionResult> ApproveSeller(int sellerId)
        {
            var seller = await _context.Sellers
                .Include(s => s.User) 
                .FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null)
                return NotFound(new { Success = false, Message = "Seller not found" });

            if (seller.IsApproved)
                return BadRequest(new { Success = false, Message = "Seller already approved" });

            seller.IsApproved = true;

            if (seller.User != null)
            {
                seller.User.Role = "Seller";

                var currentRoles = await _userManager.GetRolesAsync(seller.User);
                await _userManager.RemoveFromRolesAsync(seller.User, currentRoles);
                await _userManager.AddToRoleAsync(seller.User, "Seller");

                seller.User.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(seller.User);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Seller approved and role updated to Seller successfully." });
        }

        [HttpGet("pending-sellers")]
        public async Task<IActionResult> GetPendingSellers()
        {
            var pendingSellers = await _context.Sellers
                .Include(s => s.User)
                .Where(s => s.IsApproved == false)
                .Select(s => new
                {
                    Id = s.Id,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    StoreName = s.StoreName,
                    IsApproved = s.IsApproved
                })
                .ToListAsync();

            return Ok(new { isSuccess = true, data = pendingSellers });
        }

        [HttpDelete("reject-seller/{sellerId}")]
        public async Task<IActionResult> RejectSeller(int sellerId)
        {
            var seller = await _context.Sellers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sellerId);

            if (seller == null)
                return NotFound(new { Success = false, Message = "Seller not found" });

            // إعادة الدور إلى Customer
            if (seller.User != null)
            {
                seller.User.Role = "Customer";

                var currentRoles = await _userManager.GetRolesAsync(seller.User);
                await _userManager.RemoveFromRolesAsync(seller.User, currentRoles);
                await _userManager.AddToRoleAsync(seller.User, "Customer");

                await _userManager.UpdateAsync(seller.User);
            }

            // حذف طلب البائع
            _context.Sellers.Remove(seller);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Seller rejected and role reverted to Customer" });
        }

        [HttpGet("approved-sellers")]
        public async Task<IActionResult> GetApprovedSellers()
        {
            try
            {
                var approvedSellers = await _context.Sellers
                    .Include(s => s.User)
                    .Where(s => s.IsApproved == true)
                    .Select(s => new
                    {
                        s.Id,
                        s.UserId,
                        s.StoreName,
                        s.Description,
                        s.Logo,
                        s.IsApproved,
                        s.TotalEarnings,
                        s.CreatedAt,
                        s.UpdatedAt,
                        FullName = s.User != null ? s.User.FullName : "Unknown",
                        Email = s.User != null ? s.User.Email : "No Email",
                        PhoneNumber = s.User != null ? s.User.PhoneNumber : ""
                    })
                    .ToListAsync();

                return Ok(new { isSuccess = true, data = approvedSellers, count = approvedSellers.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = ex.Message });
            }
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
