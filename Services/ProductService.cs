using AutoMapper;
using Core.DTOs.GeneralDto;
using Core.DTOs.Product;
using Core.Helpers;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            var product = _mapper.Map<Product>(dto);
            product.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<Product>(product)
            };
        }

        public async Task<GeneralResponse> UpdateProductAsync(UpdateProductDto dto, string userId)
        {
            // Check if the product exists and belongs to the seller
            var product = await _repository.GetAsync(p => p.ProductId == dto.ProductId, "SellerProfile");
            if (product == null)
                return new GeneralResponse { IsSuccess = false, Data = "Product not found" };
            // Check if the seller is authorized to update the product
            if (product.SellerProfile?.UserId != userId)
                return new GeneralResponse { IsSuccess = false, Data = "This seller is unauthorized to update this product" };
            // Map the updated fields from the DTO to the existing product entity
            _mapper.Map(dto, product);
            // Update the product in the repository and save changes
            _repository.Update(product);
            await _repository.SaveChangesAsync();
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<Product>(product)
            };
        }
        public async Task<GeneralResponse> RemoveProductAsync(int productId, string userId)
        {
            // Check if the product exists and belongs to the seller
            var product = await _repository.GetAsync(p => p.ProductId == productId, "SellerProfile");
            if (product == null)
                return new GeneralResponse { IsSuccess = false, Data = "Product not found" };
            // Check if the seller is authorized to delete the product
            if (product.SellerProfile?.UserId != userId)
                return new GeneralResponse { IsSuccess = false, Data = "This seller is unauthorized to delete this product" };
            // Delete the product from the repository and save changes
            _repository.Delete(product);
            await _repository.SaveChangesAsync();
            return new GeneralResponse { IsSuccess = true, Data = "Product removed successfully" };
        }
        public async Task<GeneralResponse> GetProductByIdAsync(int productId)
        {
            // Check if the product exists
            var product = await _repository.GetAsync(p => p.ProductId == productId, "Category,Images,Reviews,SellerProfile");
            if (product == null)
                return new GeneralResponse { IsSuccess = false, Data = "Product not found" };
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<Product>(product)
            };
        }
        public async Task<GeneralResponse> GetAllProductsAsync()
        {
            // Retrieve all products with related entities
            var products = _repository.GetAll(includeProperties: "Category,Images,SellerProfile");
            // Map the products to DTOs and return the response
            var productDTos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return new GeneralResponse
            {
                IsSuccess = true,
                Data = _mapper.Map<List<Product>>(products)
            };
        }
        public async Task<GeneralResponse> AddProductImageAsync(int productId, IFormFile image)
        {
            // Check if the product exists
            var product = await _repository.GetAsync(p => p.ProductId == productId, includeProperties: "Images");
            if (product == null)
                return new GeneralResponse { IsSuccess = false, Data = "Product not found" };
            // Generate a unique filename for the image
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            // Determine the folder path to save the image
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, MediaSettings.ProductImagesPath);
            // Ensure the folder exists, if not create it
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            // Combine the folder path and image name to get the full path
            string fullPath = Path.Combine(folderPath, imageName);
            // Save the image to the specified path
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            // Add the image to the product's image collection
            var productImage = new ProductImage
            {
                ImageName = imageName,
                ProductId = productId,
                IsMain = !product.ImagesNames.Any() // Set as main image if it's the first one
            };
            product.ImagesNames.Add(productImage);
            await _repository.SaveChangesAsync();
            return new GeneralResponse { IsSuccess = true, Data = "Image added successfully" };
        }
    }
}