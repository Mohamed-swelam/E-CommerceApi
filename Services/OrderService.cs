using Core.DTOs.GeneralDto;
using Core.DTOs.Order;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly IProductRepository productRepository;
        private readonly IStripeService stripeService;
        private readonly IPaymentRepository paymentRepository;
        private const decimal ShippingFees = 50;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IStripeService stripeService,
            IPaymentRepository paymentRepository)
        {
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.productRepository = productRepository;
            this.stripeService = stripeService;
            this.paymentRepository = paymentRepository;
        }

        public async Task<GeneralResponse> GetAllOrdersAsync(string? userId, string? guestId)
        {
            var response = new GeneralResponse();

            try
            {
                var orders = await orderRepository
                    .GetAll(o =>
                        !string.IsNullOrEmpty(userId)
                        ? o.UserId == userId
                        : o.GuestId == guestId,
                        includeProperties: "OrderItems,OrderItems.Product,OrderItems.Product.ImagesNames")
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDtos = orders.Select(o =>
                {
                    decimal subTotal =
                        o.OrderItems.Sum(i => i.Price * i.Quantity);

                    return new OrderResponseDto
                    {
                        OrderId = o.OrderId,

                        OrderDate = o.OrderDate,

                        Status = o.Status,

                        ShippingAddress = o.ShippingAddress,

                        UserId = o.UserId,
                        GuestId = o.GuestId,

                        TotalItems =
                            o.OrderItems.Sum(i => i.Quantity),

                        SubTotal = subTotal,

                        ShippingFees = ShippingFees,

                        TotalAmount = subTotal + ShippingFees,

                        OrderItems = o.OrderItems.Select(oi =>
                            new OrderItemResponseDto
                            {
                                ProductId = oi.ProductId,

                                ProductName =
                                    oi.Product?.Name ?? "No Name",

                                Quantity = oi.Quantity,

                                Price = oi.Price,

                                ImageUrl =
                                    oi.Product?.ImagesNames
                                    .FirstOrDefault(img => img.IsMain)
                                    ?.ImageName

                                    ??

                                    oi.Product?.ImagesNames
                                    .FirstOrDefault()
                                    ?.ImageName
                            }).ToList()
                    };
                }).ToList();

                response.IsSuccess = true;
                response.Data = orderDtos;

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return response;
            }
        }

        public async Task<GeneralResponse> GetOrderByIdAsync(int id, string? userId, string? guestId)
        {
            var response = new GeneralResponse();

            try
            {
                var order = await orderRepository.GetAsync(
                    o => o.OrderId == id &&
                         !string.IsNullOrEmpty(userId)
                        ? o.UserId == userId
                        : o.GuestId == guestId,
                    includeProperties:
                    "OrderItems,OrderItems.Product,OrderItems.Product.ImagesNames");

                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";

                    return response;
                }

                decimal subTotal = order.OrderItems.Sum(i => i.Price * i.Quantity);

                var orderDto = new OrderResponseDto
                {
                    OrderId = order.OrderId,

                    OrderDate = order.OrderDate,

                    Status = order.Status,

                    ShippingAddress = order.ShippingAddress,

                    UserId = order.UserId,
                    GuestId = order.GuestId,

                    TotalItems =
                        order.OrderItems.Sum(i => i.Quantity),

                    SubTotal = subTotal,

                    ShippingFees = ShippingFees,

                    TotalAmount = subTotal + ShippingFees,

                    OrderItems = order.OrderItems.Select(oi =>
                        new OrderItemResponseDto
                        {
                            ProductId = oi.ProductId,

                            ProductName =
                                oi.Product?.Name ?? "No Name",

                            Quantity = oi.Quantity,

                            Price = oi.Price,

                            ImageUrl =
                                oi.Product?.ImagesNames
                                .FirstOrDefault(img => img.IsMain)
                                ?.ImageName

                                ??

                                oi.Product?.ImagesNames
                                .FirstOrDefault()
                                ?.ImageName
                        }).ToList()
                };

                response.IsSuccess = true;
                response.Data = orderDto;

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return response;
            }
        }

        public async Task<GeneralResponse> CreateOrderAsync(CreateOrderRequestDto dto, string? userId, string? guestId)
        {
            var response = new GeneralResponse();

            try
            {

                if (string.IsNullOrEmpty(userId))
                {
                    if (string.IsNullOrWhiteSpace(dto.GuestName))
                    {
                        response.IsSuccess = false;
                        response.Data = "Guest name is required.";

                        return response;
                    }

                    if (string.IsNullOrWhiteSpace(dto.GuestEmail))
                    {
                        response.IsSuccess = false;
                        response.Data = "Guest email is required.";

                        return response;
                    }
                }

                if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
                {
                    response.IsSuccess = false;

                    response.Data =
                        "Shipping address is required.";

                    return response;
                }

                var pendingExpiration = DateTime.UtcNow.AddMinutes(-15);

                var hasPendingOrder = await orderRepository.GetAsync(o =>
                            (
                                !string.IsNullOrEmpty(userId)
                                ? o.UserId == userId
                                : o.GuestEmail == dto.GuestEmail
                            ) && o.Status == OrderStatus.Pending
                            &&
                            o.OrderDate > pendingExpiration
                    );

                if (hasPendingOrder != null)
                {
                    response.IsSuccess = false;
                    response.Data =
                        "You already have a pending order.";

                    return response;
                }

                var cart = await cartRepository.GetAsync(
                    c => !string.IsNullOrEmpty(userId)
                        ? c.UserId == userId
                        : c.GuestId == guestId,
                    includeProperties: "Items,Items.Product");

                if (cart == null || !cart.Items.Any())
                {
                    response.IsSuccess = false;
                    response.Data = "Cart is empty.";

                    return response;
                }

                decimal totalAmount = 0;

                var orderItems = new List<OrderItem>();

                foreach (var item in cart.Items)
                {
                    if (item.Product == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid product.";

                        return response;
                    }

                    if (item.Quantity <= 0)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid quantity.";

                        return response;
                    }

                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        response.IsSuccess = false;
                        response.Data =
                            $"{item.Product.Name} out of stock.";

                        return response;
                    }

                    totalAmount += item.Product.Price * item.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                totalAmount += ShippingFees;

                var paymentIntent =
                    await stripeService
                    .CreatePaymentIntent(totalAmount);

                var order = new Order
                {
                    UserId = userId,
                    ShippingAddress = dto.ShippingAddress,
                    GuestEmail = dto.GuestEmail,
                    GuestName = dto.GuestName,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalAmount = totalAmount,
                    OrderItems = orderItems,
                    GuestId = guestId
                };

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

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;

                response.Data =
                    ex.InnerException?.Message
                    ?? ex.Message;

                return response;
            }
        }

        public async Task<GeneralResponse> CancelOrderAsync(int id, string? userId, string? guestId)
        {
            var response = new GeneralResponse();

            try
            {
                var order = await orderRepository.GetAsync(
                    o => o.OrderId == id &&
                         (
                             !string.IsNullOrEmpty(userId)
                                 ? o.UserId == userId
                                 : o.GuestId == guestId
                         ),
                    includeProperties: "Payment");

                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";

                    return response;
                }

                if (order.Status != OrderStatus.Pending)
                {
                    response.IsSuccess = false;
                    response.Data =
                        "Only pending orders can be cancelled.";

                    return response;
                }

                if (order.Payment != null)
                {
                    order.Payment.Status = PaymentStatus.Failed;

                    paymentRepository.Update(order.Payment);
                }

                order.Status = OrderStatus.Cancelled;

                orderRepository.Update(order);

                await orderRepository.SaveChangesAsync();

                response.IsSuccess = true;
                response.Data =
                    "Order cancelled successfully.";

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return response;
            }
        }

        public async Task<GeneralResponse> GetOrderStatusAsync(int id, string? userId, string? guestId)
        {
            var response = new GeneralResponse();
            try
            {
                var order = await orderRepository.GetAsync(o =>
                    o.OrderId == id &&
                    (
                        !string.IsNullOrEmpty(userId)
                            ? o.UserId == userId
                            : o.GuestId == guestId
                    ));
                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";
                    return response;
                }
                response.IsSuccess = true;
                response.Data = order.Status;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;
                return response;
            }
        }

        public async Task<GeneralResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var response = new GeneralResponse();

            try
            {
                var order = await orderRepository.GetAsync(o => o.OrderId == orderId, includeProperties: "OrderItems");

                if (order == null)
                {
                    response.IsSuccess = false;
                    response.Data = "Order not found.";

                    return response;
                }

                order.Status = dto.Status;

                orderRepository.Update(order);

                await orderRepository.SaveChangesAsync();

                response.IsSuccess = true;
                response.Data = "Order status updated successfully.";

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

                return response;
            }
        }
    }
}
