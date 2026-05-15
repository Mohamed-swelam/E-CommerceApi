using Core.DTOs.Product;
using Core.Helpers;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService service;

        public ProductController(IProductService service)
        {
            this.service = service;
        }

        // GET: api/product
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductFilterDto filter)
        {
            var result =
                await service.GetAllProductsAsync(filter);

            return Ok(result);
        }

        // GET: api/product/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var response =await service.GetProductByIdAsync(id);

            if (!response.IsSuccess)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        // POST: api/product
        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    IsSuccess = false,
                    Data = ModelState
                });
            }

            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Data = "User is not authenticated."
                });
            }

            var response =await service.AddProductAsync(dto,userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // PUT: api/product
        [HttpPut]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    IsSuccess = false,
                    Data = ModelState
                });
            }

            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Data = "User is not authenticated."
                });
            }

            var response =await service.UpdateProductAsync(dto,userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Data = "User is not authenticated."
                });
            }

            var response =await service.RemoveProductAsync(id,userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // POST: api/product/{id}/image
        [HttpPost("{id:int}/image")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> AddProductImage(int id,IFormFile image)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Data = "User is not authenticated."
                });
            }

            var response =await service.AddProductImageAsync(id,image,userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // GET: api/product/seller
        [HttpGet("seller")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> GetProductsBySeller()
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    IsSuccess = false,
                    Data = "User is not authenticated."
                });
            }

            var response =await service.GetProductsBySellerAsync(userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
