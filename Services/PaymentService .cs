using Core.DTOs.GeneralDto;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IProductRepository productRepository;
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly IConfiguration configuration;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IConfiguration configuration)
        {
            this.paymentRepository = paymentRepository;
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.configuration = configuration;
        }

        public async Task<GeneralResponse> HandleStripeWebhookAsync(
            string json,
            string stripeSignature)
        {
            var response = new GeneralResponse();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    configuration["StripeSettings:WebhookSecret"]
                );

                // Payment Success
                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent =
                        stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid payment intent.";

                        return response;
                    }

                    var payment = await paymentRepository.GetAsync(
                        p => p.TransactionId == paymentIntent.Id,
                        includeProperties:
                        "Order,Order.OrderItems");

                    if (payment == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Payment not found.";

                        return response;
                    }

                    // Prevent duplicate webhook calls
                    if (payment.Status == PaymentStatus.Paid)
                    {
                        response.IsSuccess = true;
                        response.Data = "Payment already processed.";

                        return response;
                    }

                    payment.Status = PaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;

                    paymentRepository.Update(payment);

                    if (payment.Order != null)
                    {
                        payment.Order.Status =
                            OrderStatus.Confirmed;

                        orderRepository.Update(payment.Order);

                        // Update stock
                        foreach (var item in payment.Order.OrderItems)
                        {
                            var product =
                                await productRepository.GetAsync(
                                    p => p.ProductId == item.ProductId);

                            if (product != null)
                            {
                                if (product.StockQuantity < item.Quantity)
                                {
                                    response.IsSuccess = false;
                                    response.Data = "Insufficient stock.";

                                    return response;
                                }
                                product.StockQuantity -= item.Quantity;

                                productRepository.Update(product);
                            }
                        }

                        // Clear cart
                        var cart = await cartRepository.GetAsync(
                        c =>
                            !string.IsNullOrEmpty(payment.Order.UserId)
                            ? c.UserId == payment.Order.UserId
                            : c.GuestId == payment.Order.GuestId,
                        includeProperties: "Items");

                        if (cart != null)
                        {
                            cart.Items.Clear();

                            cartRepository.Update(cart);
                        }
                    }

                    await orderRepository.SaveChangesAsync();

                    response.IsSuccess = true;
                    response.Data =
                        "Payment completed successfully.";

                    return response;
                }

                // Payment Failed
                if (stripeEvent.Type ==
                    "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Invalid payment intent.";

                        return response;
                    }

                    var payment = await paymentRepository.GetAsync(
                        p => p.TransactionId == paymentIntent.Id,
                        includeProperties: "Order");

                    if (payment == null)
                    {
                        response.IsSuccess = false;
                        response.Data = "Payment not found.";

                        return response;
                    }

                    payment.Status = PaymentStatus.Failed;

                    paymentRepository.Update(payment);

                    if (payment.Order != null)
                    {
                        payment.Order.Status =
                            OrderStatus.Cancelled;

                        orderRepository.Update(payment.Order);
                    }

                    await orderRepository.SaveChangesAsync();

                    response.IsSuccess = true;
                    response.Data = "Payment failed.";

                    return response;
                }

                response.IsSuccess = true;
                response.Data = "Unhandled event.";

                return response;
            }
            catch (StripeException ex)
            {
                response.IsSuccess = false;
                response.Data = ex.Message;

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
