using Core.Enums;
using Core.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IProductRepository productRepository;
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly IConfiguration configuration;

        public PaymentController(
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

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body)
                    .ReadToEndAsync();

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    configuration["StripeSettings:WebhookSecret"]
                );

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent =
                        stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent == null)
                    {
                        return BadRequest();
                    }

                    var payment = await paymentRepository.GetAsync(
                        p => p.TransactionId == paymentIntent.Id,
                        includeProperties:
                        "Order,Order.OrderItems");

                    if (payment == null)
                    {
                        return NotFound();
                    }

                    // Prevent Duplicate Webhook Calls
                    if (payment.Status == PaymentStatus.Paid)
                    {
                        return Ok();
                    }

                    payment.Status = PaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;

                    paymentRepository.Update(payment);

                    if (payment.Order != null)
                    {
                        payment.Order.Status =
                            OrderStatus.Confirmed;

                        orderRepository.Update(payment.Order);

                        //update stock quantity
                        foreach (var item in payment.Order.OrderItems)
                        {
                            var product =
                                await productRepository.GetAsync(
                                    p => p.ProductId == item.ProductId);

                            if (product != null)
                            {
                                product.StockQuantity -= item.Quantity;

                                productRepository.Update(product);
                            }
                        }


                        //reset cart
                        var cart = await cartRepository.GetAsync(
                            c => c.UserId == payment.Order.UserId,
                            includeProperties: "Items");

                        if (cart != null)
                        {
                            cart.Items.Clear();

                            cartRepository.Update(cart);
                        }
                    }

                    await orderRepository.SaveChangesAsync();

                    return Ok();
                }

                // Handle Payment Failure
                if (stripeEvent.Type ==
                    "payment_intent.payment_failed")
                {
                    var paymentIntent =
                        stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent == null)
                    {
                        return BadRequest();
                    }

                    var payment = await paymentRepository.GetAsync(
                        p => p.TransactionId == paymentIntent.Id,
                        includeProperties: "Order");

                    if (payment == null)
                    {
                        return NotFound();
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

                    return Ok();
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }
    }
}