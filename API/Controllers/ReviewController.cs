using Core.DTOs.Review;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{

    [ApiController]
    [Route("api/product/{productId}/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;
        public ReviewController(IReviewService service)
        {
            _service = service;
        }
        // GET: api/product/{productId}/review
        [HttpGet]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var response = await _service.GetProductReviewsAsync(productId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        // POST: api/product/{productId}/review
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, [FromBody] AddReviewDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            dto.ProductId = productId;
            var response = await _service.AddReviewAsync(dto, userId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //DELETE: api/product/{productId}/review/{reviewId}
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int productId, int reviewId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            var response = await _service.DeleteReviewAsync(reviewId, userId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
    }
}