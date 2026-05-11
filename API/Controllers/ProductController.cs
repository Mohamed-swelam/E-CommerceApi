using Core.DTOs.Product;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }
        //Get Api/product
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await _service.GetAllProductsAsync();
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Get Api/product/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var response = await _service.GetProductByIdAsync(id);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Post api/product
        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authorized" });
            var response = await _service.AddProductAsync(dto, userId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Put api/product
        [HttpPut]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authorized" });
            var response = await _service.UpdateProductAsync(dto, userId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Delete api/product/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authorized" });
            var response = await _service.RemoveProductAsync(id, userId);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Post api/product/{id}/image
        [HttpPost("{id}/image")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> AddProductImage(int id, IFormFile image)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authorized" });
            var response = await _service.AddProductImageAsync(id, image);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
    }
}