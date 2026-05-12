using AutoMapper;
using Core.DTOs.Review;
using Core.Models;
namespace Core.Mappers
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            // AddReviewDto => Review
            CreateMap<AddReviewDto, Review>();
            // Review => ReviewDto
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}