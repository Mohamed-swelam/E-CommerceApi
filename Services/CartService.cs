using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;

namespace Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository repository, IProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<CartResponseDto?> GetUserCartAsync(string userId)
        {
            var cart = await _repository.GetAsync(
                c => c.UserId == userId,
                includeProperties: "Items,Items.Product"
            );

            if (cart == null)
                return null;

            return new CartResponseDto
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "No Name",
                    Price = i.Product?.Price ?? 0,
                    Quantity = i.Quantity
                    //Remeber to add ImageUrl property to Product model and set it here
                }).ToList()
            };
        }

        public async Task<GeneralResponse> AddToCartAsync(AddToCartDto dto, string userId)
        {
            if (dto.Quantity <= 0)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Quantity must be greater than 0"
                };
            }

            var product = await _productRepository.GetAsync(p => p.ProductId == dto.ProductId);
            if (product == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Product not found"
                };
            }

            var cart = await _repository.GetAsync(c => c.UserId == userId,includeProperties:"Items");
            if (cart == null)
            {
                if (dto.Quantity > product.StockQuantity)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data = "Requested quantity exceeds available stock"
                    };
                }

                var newCart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>
                    {
                        new CartItem
                        {
                            ProductId = dto.ProductId,
                            Quantity = dto.Quantity
                        }
                    }
                };
                await _repository.AddAsync(newCart);
                await _repository.SaveChangesAsync();
            }
            else
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + dto.Quantity > product.StockQuantity)
                    {
                        return new GeneralResponse
                        {
                            IsSuccess = false,
                            Data = "Requested quantity exceeds available stock"
                        };
                    }

                    existingItem.Quantity += dto.Quantity;
                    _repository.Update(cart);
                    await _repository.SaveChangesAsync();
                }
                else
                {
                    if (dto.Quantity > product.StockQuantity)
                    {
                        return new GeneralResponse
                        {
                            IsSuccess = false,
                            Data = "Requested quantity exceeds available stock"
                        };
                    }

                    cart.Items.Add(new CartItem
                    {
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    });
                    _repository.Update(cart);
                    await _repository.SaveChangesAsync();
                }
            }

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Item Added successfully"
            };
        }

        public async Task<GeneralResponse> UpdateCartItemAsync(UpdateCartItemDto dto, string userId)
        {
            if (dto.Quantity <= 0)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Quantity must be greater than 0"
                };
            }

            var product = await _productRepository.GetAsync(p => p.ProductId == dto.ProductId);
            if (product == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Product not found"
                };
            }

            if (dto.Quantity > product.StockQuantity)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Requested quantity exceeds available stock"
                };
            }

            var cart = await _repository.GetAsync(c => c.UserId == userId, includeProperties: "Items");
            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Product not found in cart"
                };
            }

            existingItem.Quantity = dto.Quantity;
            _repository.Update(cart);
            await _repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Cart item updated successfully"
            };
        }

        public async Task<GeneralResponse> RemoveCartItemAsync(int productId, string userId)
        {
            var cart = await _repository.GetAsync(c => c.UserId == userId, includeProperties: "Items");
            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Product not found in cart"
                };
            }

            cart.Items.Remove(item);
            await _repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Cart item removed successfully"
            };
        }

        public async Task<GeneralResponse> ClearCartAsync(string userId)
        {
            var cart = await _repository.GetAsync(c => c.UserId == userId, includeProperties: "Items");
            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            foreach (var item in cart.Items.ToList())
            {
                cart.Items.Remove(item);
            }
            await _repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Cart cleared successfully"
            };
        }
    }
}
