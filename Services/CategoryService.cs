using AutoMapper;
using Core.DTOs.Category;
using Core.DTOs.GeneralDto;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<GeneralResponse> AddCategoryAsync(AddCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<CategoryDto>(category)
            };
        }

        public async Task<GeneralResponse> GetAllCategoriesAsync()
        {
            var categories = _repository.GetAll();
            var categoriesDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<IEnumerable<CategoryDto>>(categories)
            };
        }

        public async Task<GeneralResponse> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Category not found"
                };
            return new GeneralResponse { IsSuccess = true, Data = _mapper.Map<CategoryDto>(category) };
        }

        public async Task<GeneralResponse> RemoveCategoryAsync(int categoryId)
        {
            var category = await _repository.GetByIdAsync(categoryId);
            if (category == null)
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Category not found"
                };
            _repository.Delete(category);
            await _repository.SaveChangesAsync();
            return new GeneralResponse { IsSuccess = true, Data = $"Category {category.Name} removed successfully" };
        }

        public async Task<GeneralResponse> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            var category = await _repository.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Category not found"
                };
            _mapper.Map(dto, category);
            _repository.Update(category);
            await _repository.SaveChangesAsync();
            return new GeneralResponse { IsSuccess = true, Data = _mapper.Map<CategoryDto>(category) };
        }
    }
}