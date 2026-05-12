using AutoMapper;
using Core.DTOs.GeneralDto;
using Core.DTOs.Review;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
namespace Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;
        public ReviewService(IReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<GeneralResponse> AddReviewAsync(AddReviewDto dto, string userId)
        {
            var review = _mapper.Map<Review>(dto);
            review.UserId = userId;
            review.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(review);
            await _repository.SaveChangesAsync();
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<ReviewDto>(review)
            };
        }

        public async Task<GeneralResponse> DeleteReviewAsync(int reviewId, string userId)
        {
            var review = await _repository.GetAsync(r => r.ReviewId == reviewId);
            if (review == null) 
                return new GeneralResponse { IsSuccess = false, Data = "Review not found" };
            if (review.UserId != userId)
                return new GeneralResponse { IsSuccess = false, Data = "Unauthorized" };
            _repository.Delete(review);
            await _repository.SaveChangesAsync();
            return new GeneralResponse { IsSuccess = true, Data = "Review deleted successfully" };
        }

        public async Task<GeneralResponse> GetProductReviewsAsync(int productId)
        {
            var reviews = _repository.GetAll(r => r.ProductId == productId, includeProperties: "User");
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = reviewDtos
            };
        }
    }
}