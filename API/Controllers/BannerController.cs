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
        private readonly IBannerService _service;

        public BannerController(IBannerService service)
        {
            _service = service;
        }

        // GET: api/Banner
        [HttpGet]
        public async Task<IActionResult> GetActiveBanners()
        {
            var response = await _service.GetActiveBannersAsync();
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        // GET: api/Banner/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBanners()
        {
            var response = await _service.GetAllBannersAsync();
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        // GET: api/Banner/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBanner(int id)
        {
            var response = await _service.GetBannerByIdAsync(id);
            if (!response.IsSuccess)
                return NotFound(response);
            return Ok(response);
        }

        // POST: api/Banner
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBanner([FromForm] AddBannerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { IsSuccess = false, Data = ModelState });
            }

            var response = await _service.AddBannerAsync(dto);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        // PUT: api/Banner/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] UpdateBannerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { IsSuccess = false, Data = ModelState });
            }

            var response = await _service.UpdateBannerAsync(id, dto);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }

        // DELETE: api/Banner/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var response = await _service.DeleteBannerAsync(id);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
    }
}
