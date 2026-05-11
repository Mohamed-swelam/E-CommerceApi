using Core.DTOs.Seller;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SellerController : ControllerBase
    {
        private readonly ISellerService _sellerService;

        public SellerController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

       
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSellers()
        {
            var result = await _sellerService.GetAllSellersAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSellerById(int id)
        {
            var result = await _sellerService.GetSellerByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

      
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _sellerService.GetSellerProfileAsync(userId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterSeller([FromForm] RegisterSellerDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _sellerService.RegisterSellerAsync(dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateSellerDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _sellerService.UpdateSellerProfileAsync(dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Seller")]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteSeller()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _sellerService.DeleteSellerAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
