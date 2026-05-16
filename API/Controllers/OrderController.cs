using Core.DTOs.Order;
using Core.Helpers;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
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

            var result =
                await orderService.GetAllOrdersAsync(userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
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

            var result =
                await orderService.GetOrderByIdAsync(id, userId);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
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

            var guestId =
                GuestHelper.GetGuestId(HttpContext);

            var result =
                await orderService.CreateOrderAsync(dto, userId, guestId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelOrder(int id)
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

            var result =
                await orderService.CancelOrderAsync(id, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id:int}/status")]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var userId = User.GetUserId();
            var guestId = GuestHelper.GetGuestId(HttpContext);

            var result =
                await orderService.GetOrderStatusAsync(id, userId, guestId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}