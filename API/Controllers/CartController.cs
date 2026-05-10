using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;
using Core.Helpers;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService cartService;

        public CartController(ICartService cartService)
        {
            this.cartService = cartService;
        }


        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var cart = await cartService.GetUserCartAsync(userId);

            if (cart == null)
                return NotFound("Cart is empty");

            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = User.GetUserId();
            var response = new GeneralResponse();
            if (userId == null)
            {
                response = new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "User not authenticated"
                };
                return Unauthorized(response);
            }

            response = await cartService.AddToCartAsync(dto, userId);
            return Ok(response);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto dto)
        {
            var userId = User.GetUserId();
            var response = new GeneralResponse();

            if (userId == null)
            {
                response = new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "User not authenticated"
                };
                return Unauthorized(response);
            }

            response = await cartService.UpdateCartItemAsync(dto, userId);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);

        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
            var userId = User.GetUserId();
            var response = new GeneralResponse();

            if (userId == null)
            {
                response.IsSuccess = false;
                response.Data = "User not authenticated";
                return Unauthorized(response);
            }

            response = await cartService.RemoveCartItemAsync(productId, userId);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }


        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.GetUserId();
            var response = new GeneralResponse();
            if (userId == null)
            {
                response.IsSuccess = false;
                response.Data = "User not authenticated";
                return Unauthorized(response);
            }

            response = await cartService.ClearCartAsync(userId);
            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);

        }
    }
}
