using Core.DTOs.GeneralDto;

namespace Core.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<GeneralResponse> HandleStripeWebhookAsync(string json, string stripeSignature);
    }
}
