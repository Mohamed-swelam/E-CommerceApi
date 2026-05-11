using Core.DTOs.Category;
using Core.DTOs.GeneralDto;
namespace Core.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<GeneralResponse> GetAllCategoriesAsync();
        Task<GeneralResponse> GetCategoryByIdAsync(int categoryId);
        Task<GeneralResponse> AddCategoryAsync(AddCategoryDto dto);
        Task<GeneralResponse> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<GeneralResponse> RemoveCategoryAsync(int categoryId);
    }
}