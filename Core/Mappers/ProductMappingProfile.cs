using AutoMapper;
using Core.DTOs.Product;
using Core.Models;
namespace Core.Mappers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            //AddProductDto to Product
            CreateMap<AddProductDto, Product>();
            //UpdateProductDto to Product with condition to ignore null values
            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            //Product to ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.StoreName,
                opt => opt.MapFrom(src => src.SellerProfile.StoreName))
                .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.ImagesNames,
                opt => opt.MapFrom(src => src.ImagesNames.Select(i => i.ImageName).ToList()));
        }
    }
}