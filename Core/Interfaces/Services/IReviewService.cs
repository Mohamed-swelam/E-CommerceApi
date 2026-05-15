using Core.DTOs.GeneralDto;
using Core.DTOs.Review;
namespace Core.Interfaces.Services
{
    public interface IReviewService
    {
        Task<GeneralResponse> GetProductReviewsAsync(int productId);
        Task<GeneralResponse> AddReviewAsync(AddReviewDto dto, string userId);
        Task<GeneralResponse> UpdateReviewAsync(int productId, int reviewId, UpdateReviewDto dto, string userId);
        Task<GeneralResponse> DeleteReviewAsync(int reviewId, string userId);
    }
}