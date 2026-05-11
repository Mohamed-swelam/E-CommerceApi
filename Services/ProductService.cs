using AutoMapper;
using Core.DTOs.GeneralDto;
using Core.DTOs.Product;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository repository, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }
        public async Task<GeneralResponse> AddProductAsync(AddProductDto dto, string userId)
        {
            var product = _mapper.Map<ProductDto>(dto);
            product.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(product);
        }

        public Task<GeneralResponse> AddProductImageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> GetAllProductsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> GetProductByIDAsync(int ProductId)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> RemoveProductAsync(int productId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> UpdateProductAsync(UpdateProductDto dto, string userId)
        {
            throw new NotImplementedException();
        }
    }
}