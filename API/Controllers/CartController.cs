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

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var cart =
                await cartService.GetUserCartAsync(
                    userId,
                    guestId);

            if (cart == null)
                return NotFound("Cart is empty");

            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(
            AddToCartDto dto)
        {
            var userId = User.GetUserId();

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var response =
                await cartService.AddToCartAsync(
                    dto,
                    userId,
                    guestId);

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCartItem(
            UpdateCartItemDto dto)
        {
            var userId = User.GetUserId();

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var response =
                await cartService.UpdateCartItemAsync(
                    dto,
                    userId,
                    guestId);

            return Ok(response);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveCartItem(
            int productId)
        {
            var userId = User.GetUserId();

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var response =
                await cartService.RemoveCartItemAsync(
                    productId,
                    userId,
                    guestId);

            return Ok(response);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.GetUserId();

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var response =
                await cartService.ClearCartAsync(
                    userId,
                    guestId);

            return Ok(response);
        }
    }
}
