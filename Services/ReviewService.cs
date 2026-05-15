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
            var existing = await _repository.GetAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
            if (existing != null)
            {
                return new GeneralResponse { IsSuccess = false, Data = "You already reviewed this product." };
            }

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

        public async Task<GeneralResponse> UpdateReviewAsync(int productId, int reviewId, UpdateReviewDto dto, string userId)
        {
            var review = await _repository.GetAsync(
                r => r.ReviewId == reviewId,
                includeProperties: "User"
            );

            if (review == null)
                return new GeneralResponse { IsSuccess = false, Data = "Review not found" };

            if (review.UserId != userId)
                return new GeneralResponse { IsSuccess = false, Data = "Unauthorized" };

            if (review.ProductId != productId)
                return new GeneralResponse { IsSuccess = false, Data = "Invalid product review" };

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            _repository.Update(review);
            await _repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<ReviewDto>(review)
            };
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