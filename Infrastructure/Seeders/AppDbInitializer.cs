using Core.Models;
using Infrastructure.Data;
using Infrastructure.Seeders.SeedingHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Seeders
{
    public static class AppDbInitializer
    {
        public static async Task SeedAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Seller", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            if (!userManager.Users.Any())
            {
                var usersPath = Path.Combine(
    Directory.GetCurrentDirectory(),
                    "..",
                    "Infrastructure",
    "Seeders",
    "Dummy",
    "Users.json");

                var usersData =
                    await ReadJsonAsync<UsersSeedRoot>(
                        usersPath);

                var allUsers = new List<SeedUserDto>();

                allUsers.AddRange(usersData.Sellers);
                allUsers.AddRange(usersData.Customers);
                allUsers.Add(usersData.Admin);

                foreach (var item in allUsers)
                {
                    var user = new ApplicationUser
                    {
                        UserName = item.UserName,
                        FullName = item.FullName,
                        Email = item.Email,
                        EmailConfirmed = item.EmailConfirmed
                    };

                    var result = await userManager.CreateAsync(
                        user,
                        item.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            user,
                            item.Role);
                    }
                }
            }

            // Always sync categories from JSON (so icon changes are picked up)
            var categoriesPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "Infrastructure",
                "Seeders",
                "Dummy",
                "Categories.json");

            var jsonCategories =
                await ReadJsonAsync<List<Category>>(
                    categoriesPath);

            if (jsonCategories != null)
            {
                foreach (var jsonCategory in jsonCategories)
                {
                    var existing = await context.Categories
                        .FirstOrDefaultAsync(c => c.Name == jsonCategory.Name);

                    if (existing != null)
                    {
                        // Update existing category (especially icon)
                        existing.Icon = jsonCategory.Icon;
                        context.Categories.Update(existing);
                    }
                    else
                    {
                        // Add new category if it doesn't exist
                        context.Categories.Add(jsonCategory);
                    }
                }

                await context.SaveChangesAsync();
            }

            if (!context.Sellers.Any())
            {
                var sellerProfilesPath = Path.Combine(
    Directory.GetCurrentDirectory(),
                    "..",
                    "Infrastructure",
    "Seeders",
    "Dummy",
    "SellerProfiles.json");

                var sellerProfiles =
                    await ReadJsonAsync<List<SellerProfileSeedDto>>(
                        sellerProfilesPath);

                foreach (var item in sellerProfiles)
                {
                    var user =
                        await userManager.FindByEmailAsync(
                            item.UserEmail);

                    if (user == null)
                        continue;

                    var sellerProfile = new Sellerprofile
                    {
                        StoreName = item.StoreName,
                        Logo = item.Logo,
                        Description = item.Description,
                        UserId = user.Id,
                        IsApproved = item.IsApproved,
                        TotalEarnings = item.TotalEarnings
                    };

                    context.Sellers.Add(sellerProfile);
                }

                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                var productsPath = Path.Combine(
    Directory.GetCurrentDirectory(),
                    "..",
                    "Infrastructure",
    "Seeders",
    "Dummy",
    "Products.json");

                var products =
                    await ReadJsonAsync<List<ProductSeedDto>>(
                        productsPath);

                foreach (var item in products)
                {
                    var sellerProfile = await context.Sellers
                        .FirstOrDefaultAsync(s =>
                            s.StoreName ==
                            item.SellerStoreName);

                    if (sellerProfile == null)
                        continue;

                    var product = new Product
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        StockQuantity = item.StockQuantity,
                        CategoryId = item.CategoryId,
                        SellerProfileId =
                            sellerProfile.Id,

                        ImagesNames = item.ImagesNames
                            .Select(i => new ProductImage
                            {
                                ImageName = i.ImageName,
                                IsMain = i.IsMain
                            })
                        .ToList()
                    };

                    context.Products.Add(product);
                }

                await context.SaveChangesAsync();
            }

            var bannersPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "Infrastructure",
                "Seeders",
                "Dummy",
                "Banners.json");

            var jsonBanners = await ReadJsonAsync<List<Banner>>(bannersPath);
            if (jsonBanners != null)
            {
                foreach (var jsonBanner in jsonBanners)
                {
                    var existing = await context.Banners
                        .FirstOrDefaultAsync(b => b.CategoryId == jsonBanner.CategoryId && b.Title == jsonBanner.Title);

                    if (existing != null)
                    {
                        if (existing.Image != jsonBanner.Image || existing.IsActive != jsonBanner.IsActive)
                        {
                            existing.Image = jsonBanner.Image;
                            existing.IsActive = jsonBanner.IsActive;
                            context.Banners.Update(existing);
                        }
                    }
                    else
                    {
                        context.Banners.Add(jsonBanner);
                    }
                }
                await context.SaveChangesAsync();
            }


        }


        private static async Task<T?> ReadJsonAsync<T>(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File Not Found: {path}");
                return default;
            }

            var json = await File.ReadAllTextAsync(path);

            Console.WriteLine(json);

            return JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
