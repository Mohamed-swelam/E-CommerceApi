using Core.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders
{
    public static class AppDbInitializer
    {
        public static async Task SeedAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(
                    new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Seller"))
                await roleManager.CreateAsync(
                    new IdentityRole("Seller"));

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(
                    new IdentityRole("Customer"));

            //users
            var sellersCount = await userManager
                .GetUsersInRoleAsync("Seller");
            if (!sellersCount.Any())
            {
                // Seller 1
                var seller1 = new ApplicationUser
                {
                    UserName = "seller1@gmail.com",
                    Email = "seller1@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(
                    seller1,
                    "Seller@123");

                await userManager.AddToRoleAsync(
                    seller1,
                    "Seller");

                // Seller 2
                var seller2 = new ApplicationUser
                {
                    UserName = "seller2@gmail.com",
                    Email = "seller2@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(
                    seller2,
                    "Seller@123");

                await userManager.AddToRoleAsync(
                    seller2,
                    "Seller");

                // Customer 1
                var customer1 = new ApplicationUser
                {
                    UserName = "user1@gmail.com",
                    Email = "user1@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(
                    customer1,
                    "User@123");

                await userManager.AddToRoleAsync(
                    customer1,
                    "Customer");

                // Customer 2
                var customer2 = new ApplicationUser
                {
                    UserName = "user2@gmail.com",
                    Email = "user2@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(
                    customer2,
                    "User@123");

                await userManager.AddToRoleAsync(
                    customer2,
                    "Customer");

                //seller profiles

                var sellerProfile1 = new Sellerprofile
                {
                    StoreName = "Tech Store",
                    UserId = seller1.Id
                };

                var sellerProfile2 = new Sellerprofile
                {
                    StoreName = "Mobile Shop",
                    UserId = seller2.Id
                };

                context.Sellers.AddRange(
                    sellerProfile1,
                    sellerProfile2);

                await context.SaveChangesAsync();

                //categories

                var electronics = new Category
                {
                    Name = "Electronics"
                };

                var fashion = new Category
                {
                    Name = "Fashion"
                };

                context.Categories.AddRange(
                    electronics,
                    fashion);

                await context.SaveChangesAsync();

                //products

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "IPhone 15",
                        Description = "Apple mobile phone",
                        Price = 50000,
                        StockQuantity = 10,
                        CategoryId = electronics.CategoryId,
                        SellerProfileId = sellerProfile1.Id
                    },

                    new Product
                    {
                        Name = "Samsung S24",
                        Description = "Samsung mobile phone",
                        Price = 42000,
                        StockQuantity = 15,
                        CategoryId = electronics.CategoryId,
                        SellerProfileId = sellerProfile1.Id
                    },

                    new Product
                    {
                        Name = "T-Shirt",
                        Description = "Cotton T-Shirt",
                        Price = 500,
                        StockQuantity = 50,
                        CategoryId = fashion.CategoryId,
                        SellerProfileId = sellerProfile2.Id
                    }
                };

                context.Products.AddRange(products);

                await context.SaveChangesAsync();
            }
        }
    }
}
