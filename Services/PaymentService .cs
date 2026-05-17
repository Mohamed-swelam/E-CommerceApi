using Core.DTOs.GeneralDto;
using Core.Enums;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
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
        private readonly IEmailService emailService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IConfiguration configuration, IEmailService emailService)
        {
            this.paymentRepository = paymentRepository;
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.configuration = configuration;
            this.emailService = emailService;
        }

        public async Task<GeneralResponse> HandleStripeWebhookAsync(string json, string stripeSignature)
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
                        "Order,Order.OrderItems,Order.User");

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
                        payment.Order.Status = OrderStatus.Confirmed;

                        orderRepository.Update(payment.Order);
                        await SendOrderConfirmationEmailAsync(payment.Order);

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

        private async Task SendOrderConfirmationEmailAsync(Order order)
        {
            string? customerEmail = null;

            if (!string.IsNullOrEmpty(order.GuestEmail))
            {
                customerEmail = order.GuestEmail;
            }
            else if (order.User != null)
            {
                customerEmail = order.User.Email;
            }

            if (string.IsNullOrEmpty(customerEmail))
            {
                return;
            }

            var subject =
                $"Order #{order.OrderId} Confirmed";

            var body = $@"
                <!DOCTYPE html>
                <html>

                <head>

                    <style>

                        body {{
                            margin: 0;
                            padding: 0;
                            background: #f4f4f4;
                            font-family: Arial, sans-serif;
                        }}

                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background: #ffffff;
                            border-radius: 18px;
                            overflow: hidden;
                            box-shadow:
                                0 10px 30px rgba(0,0,0,.08);
                        }}

                        .header {{
                            background:
                                linear-gradient(
                                    135deg,
                                    #c8602a,
                                    #e07840
                                );

                            padding: 40px 20px;
                            text-align: center;
                            color: white;
                        }}

                        .header h1 {{
                            margin: 0;
                            font-size: 32px;
                        }}

                        .content {{
                            padding: 35px;
                            color: #333;
                        }}

                        .content h2 {{
                            margin-top: 0;
                            color: #111;
                            font-size: 26px;
                        }}

                        .info-box {{
                            background: #f9fafb;
                            border: 1px solid #e5e7eb;
                            border-radius: 14px;
                            padding: 20px;
                            margin: 25px 0;
                        }}

                        .info-row {{
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 14px;
                            font-size: 15px;
                        }}

                        .info-row:last-child {{
                            margin-bottom: 0;
                        }}

                        .label {{
                            color: #666;
                            font-weight: 600;
                        }}

                        .value {{
                            color: #111;
                            font-weight: 700;
                        }}

                        .status {{
                            color: #16a34a;
                        }}

                        .footer {{
                            text-align: center;
                            padding: 24px;
                            font-size: 13px;
                            color: #888;
                            border-top: 1px solid #eee;
                        }}

                        .button {{
                            display: inline-block;
                            margin-top: 25px;
                            padding: 14px 24px;
                            background: #c8602a;
                            color: white !important;
                            text-decoration: none;
                            border-radius: 12px;
                            font-weight: bold;
                        }}

                    </style>

                </head>

                <body>

                    <div class='container'>

                        <div class='header'>

                            <h1>
                                Order Confirmed ✅
                            </h1>

                        </div>

                        <div class='content'>

                            <h2>
                                Thank you for your purchase
                            </h2>

                            <p>
                                Your payment was successful and
                                we are preparing your order now.
                            </p>

                            <div class='info-box'>

                                <div class='info-row'>
                                    <span class='label'>
                                        Order Number
                                    </span>

                                    <span class='value'>
                                        #{order.OrderId}
                                    </span>
                                </div>

                                <div class='info-row'>
                                    <span class='label'>
                                        Total Amount
                                    </span>

                                    <span class='value'>
                                        ${order.TotalAmount}
                                    </span>
                                </div>

                                <div class='info-row'>
                                    <span class='label'>
                                        Status
                                    </span>

                                    <span class='value status'>
                                        {order.Status}
                                    </span>
                                </div>

                                <div class='info-row'>
                                    <span class='label'>
                                        Order Date
                                    </span>

                                    <span class='value'>
                                        {order.OrderDate:dd MMM yyyy}
                                    </span>
                                </div>

                            </div>

                            <p>
                                We will notify you once your order
                                is shipped 🚚
                            </p>

                        </div>

                        <div class='footer'>

                            Ecommerce Store © 2026

                        </div>

                    </div>

                </body>

                </html>
                ";

            await emailService.SendEmailAsync(
                customerEmail,
                subject,
                body
            );
        }
    }
}
