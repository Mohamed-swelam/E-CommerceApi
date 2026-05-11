using Stripe;

namespace Core.Interfaces.Services
{
    public interface IStripeService
    {
        Task<PaymentIntent> CreatePaymentIntent(decimal amount);
    }
}
