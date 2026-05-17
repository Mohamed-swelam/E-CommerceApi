using Core.DTOs.Banner;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveBanners()
        {
            var response = await _bannerService.GetActiveBannersAsync();
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBanners()
        {
            var response = await _bannerService.GetAllBannersAsync();
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBannerById(int id)
        {
            var response = await _bannerService.GetBannerByIdAsync(id);
            if (!response.IsSuccess) return NotFound(response);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBanner([FromForm] AddBannerDto addBannerDto)
        {
            var response = await _bannerService.AddBannerAsync(addBannerDto);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] UpdateBannerDto updateBannerDto)
        {
            var response = await _bannerService.UpdateBannerAsync(id, updateBannerDto);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var response = await _bannerService.DeleteBannerAsync(id);
            if (!response.IsSuccess) return BadRequest(response);
            return Ok(response);
        }
    }
}
