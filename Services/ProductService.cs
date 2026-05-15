using AutoMapper;
using Core.DTOs.GeneralDto;
using Core.DTOs.Product;
using Core.Helpers;
using Core.Interfaces.Helpers;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;
        private readonly IImageHelper imageHelper;
        private readonly IMapper mapper;
        private readonly AppDbContext context;

        public ProductService(
            IProductRepository repository,
            IImageHelper imageHelper,
            IMapper mapper,
            AppDbContext context)
        {
            this.repository = repository;
            this.imageHelper = imageHelper;
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<GeneralResponse> AddProductAsync(AddProductDto dto, string userId)
        {
            try
            {
                if (dto.Price <= 0)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Price must be greater than 0"
                    };
                }

                if (dto.StockQuantity < 0)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Invalid stock quantity"
                    };
                }

                var seller =await context.Sellers.FindAsync(dto.SellerId);

                if (seller == null)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Seller not found"
                    };
                }

                if (seller.UserId != userId)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "You are not authorized to add products for this seller"
                    };
                }

                var product = mapper.Map<Product>(dto);

                product.SellerProfileId = dto.SellerId;

                product.CreatedAt = DateTime.UtcNow;

                await repository.AddAsync(product);

                await repository.SaveChangesAsync();

                var productDto =
                    mapper.Map<ProductDto>(product);

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = productDto
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> UpdateProductAsync(UpdateProductDto dto, string userId)
        {
            try
            {
                var product = await repository.GetAsync(
                    p => p.ProductId == dto.ProductId,
                    includeProperties:
                    "SellerProfile,ImagesNames,Category");

                if (product == null)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Product not found"
                    };
                }

                if (product.SellerProfile == null || product.SellerProfile.UserId != userId)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "You are not authorized to update this product"
                    };
                }

                if (dto.Price.HasValue && dto.Price <= 0)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Price must be greater than 0"
                    };
                }

                if (dto.StockQuantity.HasValue &&
                    dto.StockQuantity < 0)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Invalid stock quantity"
                    };
                }

                mapper.Map(dto, product);

                repository.Update(product);

                await repository.SaveChangesAsync();

                var productDto = mapper.Map<ProductDto>(product);

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = productDto
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> RemoveProductAsync(int productId, string userId)
        {
            try
            {
                var product = await repository.GetAsync( p => p.ProductId == productId,includeProperties:"SellerProfile");

                if (product == null)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Product not found"
                    };
                }

                if (product.SellerProfile == null || product.SellerProfile.UserId != userId)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "You are not authorized to delete this product"
                    };
                }

                repository.Delete(product);

                await repository.SaveChangesAsync();

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = "Product removed successfully"
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> GetProductByIdAsync(int productId)
        {
            try
            {
                var product = await repository.GetAsync( p => p.ProductId == productId,
                    includeProperties:"Category,ImagesNames,Reviews,SellerProfile");

                if (product == null)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Product not found"
                    };
                }

                var productDto =
                    mapper.Map<ProductDto>(product);

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = productDto
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> GetAllProductsAsync(ProductFilterDto filter)
        {
            try
            {
                var query = repository.GetAll(includeProperties:"Category,ImagesNames,SellerProfile");

                // Search
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search =filter.Search.ToLower();

                    query = query.Where(p =>p.Name.ToLower().Contains(search));
                }

                // Category Filter
                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(p =>p.CategoryId ==filter.CategoryId.Value);
                }

                // Price Filter
                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(p =>p.Price >= filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(p =>p.Price <= filter.MaxPrice.Value);
                }

                // In Stock Only
                query = query.Where(p => p.StockQuantity > 0);

                // Sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "price" =>
                        filter.Order?.ToLower() == "asc"
                        ? query.OrderBy(p => p.Price)
                        : query.OrderByDescending(p => p.Price),

                    "name" =>
                        filter.Order?.ToLower() == "asc"
                        ? query.OrderBy(p => p.Name)
                        : query.OrderByDescending(p => p.Name),

                    _ => query.OrderByDescending(
                        p => p.CreatedAt)
                };

                var totalCount =
                    await query.CountAsync();

                var products = await query
                    .Skip((filter.Page - 1)
                        * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var productDtos =mapper.Map<IEnumerable<ProductDto>>(products);

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = new
                    {
                        TotalCount = totalCount,
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        Products = productDtos
                    }
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> AddProductImageAsync(int productId, IFormFile image, string userId)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Invalid image"
                    };
                }

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension =Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Invalid image format"
                    };
                }

                if (image.Length > 2 * 1024 * 1024)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "Image size must not exceed 2 MB"
                    };
                }

                var product = await repository.GetAsync(
                    p => p.ProductId == productId,
                    includeProperties:
                    "ImagesNames,SellerProfile");

                if (product == null)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Product not found"
                    };
                }

                if (product.SellerProfile == null || product.SellerProfile.UserId != userId)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "You are not authorized to add images to this product"
                    };
                }

                string imageName = await imageHelper.SaveImageAsync(image, MediaSettings.ProductImagesPath);

                if (product.ImagesNames == null)
                {
                    product.ImagesNames =
                        new List<ProductImage>();
                }

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageName = imageName,
                    IsMain = !product.ImagesNames.Any()
                };



                product.ImagesNames.Add(productImage);

                repository.Update(product);

                await repository.SaveChangesAsync();

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = "Image added successfully"
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }

        public async Task<GeneralResponse> GetProductsBySellerAsync(string sellerProfileId)
        {
            try
            {
                var products = await repository
                    .GetAll(p => p.SellerProfile.UserId == sellerProfileId, includeProperties: "Category,ImagesNames,SellerProfile")
                    .ToListAsync();

                var productDtos = mapper.Map<IEnumerable<ProductDto>>(products);

                return new GeneralResponse
                {
                    IsSuccess = true,
                    Data = productDtos
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = ex.Message
                };
            }
        }
    }
}
