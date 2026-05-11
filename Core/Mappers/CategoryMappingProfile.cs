using AutoMapper;
using Core.DTOs.Category;
using Core.Models;
namespace Core.Mappers
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Category → CategoryDto
            CreateMap<Category, CategoryDto>();
            // AddCategoryDto → Category
            CreateMap<AddCategoryDto, Category>()
                .ForMember(dest => dest.Icon, opt => opt.Ignore());
            // UpdateCategoryDto → Category
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));
        }
    }
}