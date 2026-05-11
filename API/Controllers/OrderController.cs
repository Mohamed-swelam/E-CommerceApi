using Core.DTOs.GeneralDto;
using Core.DTOs.Order;
using Core.Enums;
using Core.Helpers;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly IProductRepository productRepository;
        private readonly IStripeService stripeService;
        private readonly IPaymentRepository paymentRepository;

        public OrderController(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository, IStripeService stripeService, IPaymentRepository paymentRepository)
        {
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.productRepository = productRepository;
            this.stripeService = stripeService;
            this.paymentRepository = paymentRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var response = new GeneralResponse();

            try
            {
                var userId = User.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    response.IsSuccess = false;
                    response.Data = "User is not authenticated.";

                    return Unauthorized(response);
                }

                var orders = await orderRepository
                    .GetAll(
                        o => o.UserId == userId,
                        includeProperties: "OrderItems,OrderItems.Product")
                    .ToListAsync();

                var orderResponseDtos = orders.Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    UserId = o.UserId,

                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? string.Empty,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()

                }).ToList();

                response.IsSuccess = true;
                response.Data = orderResponseDtos;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return StatusCode(500, response);
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var response = new GeneralResponse();

            try
            {
                var userId = User.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    response.IsSuccess = false;
                    response.Data = "User is not authenticated.";

                    return Unauthorized(response);
                }

                var order = await orderRepository.GetAsync(
                    o => o.OrderId == id && o.UserId == userId,
                    includeProperties: "OrderItems,OrderItems.Product");

                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";

                    return NotFound(response);
                }

                var orderDto = new OrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippingAddress = order.ShippingAddress,
                    UserId = order.UserId,

                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? string.Empty,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                };

                response.IsSuccess = true;
                response.Data = orderDto;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return StatusCode(500, response);
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
        {
            var response = new GeneralResponse();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.Data = ModelState;

                    return BadRequest(response);
                }

                var userId = User.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    response.IsSuccess = false;
                    response.Data = "User not authenticated.";

                    return Unauthorized(response);
                }

                // prevent duplicate pending orders
                var hasPendingOrder = await orderRepository.GetAsync(
                    o => o.UserId == userId &&
                         o.Status == OrderStatus.Pending);

                if (hasPendingOrder != null)
                {
                    response.IsSuccess = false;
                    response.Data =
                        "You already have a pending order.";

                    return BadRequest(response);
                }

                var cart = await cartRepository.GetAsync(
                    c => c.UserId == userId,
                    includeProperties: "Items,Items.Product");

                if (cart == null || !cart.Items.Any())
                {
                    response.IsSuccess = false;
                    response.Data = "Cart is empty.";

                    return BadRequest(response);
                }

                decimal totalAmount = 0;

                var orderItems = new List<OrderItem>();

                foreach (var item in cart.Items)
                {
                    if (item.Product == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid product.";

                        return BadRequest(response);
                    }

                    if (item.Quantity <= 0)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid quantity.";

                        return BadRequest(response);
                    }

                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        response.IsSuccess = false;
                        response.Data =
                            $"{item.Product.Name} out of stock.";

                        return BadRequest(response);
                    }

                    totalAmount +=
                        item.Product.Price * item.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                // create stripe payment intent
                var paymentIntent =
                    await stripeService.CreatePaymentIntent(totalAmount);

                // create order
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = dto.ShippingAddress,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    OrderItems = orderItems
                };

                // create payment
                order.Payment = new Payment
                {
                    Amount = totalAmount,
                    TransactionId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentMethod = "Card",
                    Status = PaymentStatus.Pending
                };

                await orderRepository.AddAsync(order);

                await orderRepository.SaveChangesAsync();

                response.IsSuccess = true;

                response.Data = new
                {
                    OrderId = order.OrderId,
                    ClientSecret = paymentIntent.ClientSecret,
                    TotalAmount = order.TotalAmount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return StatusCode(500, response);
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var response = new GeneralResponse();

            try
            {
                var userId = User.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    response.IsSuccess = false;
                    response.Data = "User is not authenticated.";

                    return Unauthorized(response);
                }

                var order = await orderRepository.GetAsync(
                    o => o.OrderId == id && o.UserId == userId,
                    includeProperties: "Payment");

                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";

                    return NotFound(response);
                }

                // only pending orders can be cancelled
                if (order.Status != OrderStatus.Pending)
                {
                    response.IsSuccess = false;
                    response.Data =
                        "Only pending orders can be cancelled.";

                    return BadRequest(response);
                }

                // cancel payment if exists
                if (order.Payment != null)
                {
                    order.Payment.Status = PaymentStatus.Failed;

                    paymentRepository.Update(order.Payment);
                }

                // cancel order
                order.Status = OrderStatus.Cancelled;

                orderRepository.Update(order);

                await orderRepository.SaveChangesAsync();

                response.IsSuccess = true;
                response.Data = "Order cancelled successfully.";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return StatusCode(500, response);
            }
        }
    }
}