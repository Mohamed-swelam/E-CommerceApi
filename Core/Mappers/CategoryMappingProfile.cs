using AutoMapper;
using Core.DTOs.Category;
using Core.Models;
namespace Core.Mappers
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Mapping from Category to CategoryDto
            CreateMap<Category, CategoryDto>();
            // Mapping from AddCategoryDto to Category
            CreateMap<AddCategoryDto, Category>();
            // Mapping from UpdateCategory to Category
            CreateMap<UpdateCategory, Category>()
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));
        }
    }
}