using Core.DTOs.Cart;
using Core.DTOs.GeneralDto;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Core.Models;

namespace Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository repository;
        private readonly IProductRepository productRepository;

        public CartService(
            ICartRepository repository,
            IProductRepository productRepository)
        {
            this.repository = repository;
            this.productRepository = productRepository;
        }

        public async Task<CartResponseDto?> GetUserCartAsync(
            string? userId,
            string? guestId)
        {
            var cart = await repository.GetAsync(
                c =>
                    !string.IsNullOrEmpty(userId)
                    ? c.UserId == userId
                    : c.GuestId == guestId,

                includeProperties:
                "Items,Items.Product,Items.Product.ImagesNames"
            );

            if (cart == null)
                return null;

            return new CartResponseDto
            {
                CartId = cart.CartId,

                TotalItems =
                    cart.Items.Sum(i => i.Quantity),

                Items = cart.Items.Select(i =>
                    new CartItemResponseDto
                    {
                        ProductId = i.ProductId,

                        ProductName =
                            i.Product?.Name ?? "No Name",

                        Price =
                            i.Product?.Price ?? 0,

                        Quantity = i.Quantity,

                        ImageUrl =
                            i.Product?.ImagesNames
                            .FirstOrDefault(img => img.IsMain)
                            ?.ImageName

                            ??

                            i.Product?.ImagesNames
                            .FirstOrDefault()
                            ?.ImageName ?? ""
                    }).ToList()
            };
        }

        public async Task<GeneralResponse> AddToCartAsync(
            AddToCartDto dto,
            string? userId,
            string? guestId)
        {
            if (dto.Quantity <= 0)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Quantity must be greater than 0"
                };
            }

            var product = await productRepository.GetAsync(
                p => p.ProductId == dto.ProductId);

            if (product == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Product not found"
                };
            }

            var cart = await repository.GetAsync(
                c =>
                    !string.IsNullOrEmpty(userId)
                    ? c.UserId == userId
                    : c.GuestId == guestId,

                includeProperties: "Items"
            );

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,

                    GuestId = guestId,

                    Items = new List<CartItem>()
                };

                await repository.AddAsync(cart);
            }

            var existingItem = cart.Items
                .FirstOrDefault(i =>
                    i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + dto.Quantity >
                    product.StockQuantity)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "Requested quantity exceeds stock"
                    };
                }

                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                if (dto.Quantity > product.StockQuantity)
                {
                    return new GeneralResponse
                    {
                        IsSuccess = false,
                        Data =
                        "Requested quantity exceeds stock"
                    };
                }

                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            repository.Update(cart);

            await repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Item added successfully"
            };
        }

        public async Task<GeneralResponse> UpdateCartItemAsync(
            UpdateCartItemDto dto,
            string? userId,
            string? guestId)
        {
            var cart = await repository.GetAsync(
                c =>
                    !string.IsNullOrEmpty(userId)
                    ? c.UserId == userId
                    : c.GuestId == guestId,

                includeProperties: "Items"
            );

            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            var item = cart.Items.FirstOrDefault(
                i => i.ProductId == dto.ProductId);

            if (item == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Item not found"
                };
            }

            item.Quantity = dto.Quantity;

            repository.Update(cart);

            await repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Cart updated successfully"
            };
        }

        public async Task<GeneralResponse> RemoveCartItemAsync(
            int productId,
            string? userId,
            string? guestId)
        {
            var cart = await repository.GetAsync(
                c =>
                    !string.IsNullOrEmpty(userId)
                    ? c.UserId == userId
                    : c.GuestId == guestId,

                includeProperties: "Items"
            );

            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            var item = cart.Items.FirstOrDefault(
                i => i.ProductId == productId);

            if (item == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Item not found"
                };
            }

            cart.Items.Remove(item);

            repository.Update(cart);

            await repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Item removed successfully"
            };
        }

        public async Task<GeneralResponse> ClearCartAsync(
            string? userId,
            string? guestId)
        {
            var cart = await repository.GetAsync(
                c =>
                    !string.IsNullOrEmpty(userId)
                    ? c.UserId == userId
                    : c.GuestId == guestId,

                includeProperties: "Items"
            );

            if (cart == null)
            {
                return new GeneralResponse
                {
                    IsSuccess = false,
                    Data = "Cart not found"
                };
            }

            cart.Items.Clear();

            repository.Update(cart);

            await repository.SaveChangesAsync();

            return new GeneralResponse
            {
                IsSuccess = true,
                Data = "Cart cleared successfully"
            };
        }


        public async Task MergeGuestCartAsync(string userId, string guestId)
        {
            // Guest cart
            var guestCart = await repository.GetAsync(
                c => c.GuestId == guestId,
                includeProperties: "Items");

            if (guestCart == null)
                return;

            // User cart
            var userCart = await repository.GetAsync(
                c => c.UserId == userId,
                includeProperties: "Items");

            if (userCart == null)
            {
                guestCart.UserId = userId;

                guestCart.GuestId = null;

                repository.Update(guestCart);

                await repository.SaveChangesAsync();

                return;
            }

            // merge items
            foreach (var guestItem in guestCart.Items)
            {
                var existingItem =
                    userCart.Items.FirstOrDefault(
                        i => i.ProductId == guestItem.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity +=
                        guestItem.Quantity;
                }
                else
                {
                    userCart.Items.Add(new CartItem
                    {
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity
                    });
                }
            }

            repository.Update(userCart);

            repository.Delete(guestCart);

            await repository.SaveChangesAsync();
        }
    }
}