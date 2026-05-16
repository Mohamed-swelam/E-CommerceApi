using Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration configuration;

        public StripeService(IConfiguration configuration)
        {
            this.configuration = configuration;

            StripeConfiguration.ApiKey =
                configuration["StripeSettings:SecretKey"];
        }

        public async Task<PaymentIntent> CreatePaymentIntent(decimal amount)
        {
            if (amount <= 0)
            {
                throw new Exception(
                    "Invalid payment amount."
                );
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),

                Currency = "usd",

                PaymentMethodTypes =
                new List<string>
                {
                    "card"
                },

                Metadata = new Dictionary<string, string>
                {
                    { "integration", "ecommerce-api" }
                }
            };

            var service = new PaymentIntentService();

            return await service.CreateAsync(options);
        }
    }
}
