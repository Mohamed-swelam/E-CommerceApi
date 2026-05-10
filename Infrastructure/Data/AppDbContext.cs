using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "Seller", NormalizedName = "SELLER" },
            new IdentityRole { Id = "3", Name = "Customer", NormalizedName = "CUSTOMER" }
            );

            // ===== Seed Admin User =====
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "seed-admin-001",
                FullName = "Super Admin",
                UserName = "admin@store.com",
                NormalizedUserName = "ADMIN@STORE.COM",
                Email = "admin@store.com",
                NormalizedEmail = "ADMIN@STORE.COM",
                EmailConfirmed = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1),
                SecurityStamp = "STATIC-SECURITY-STAMP-001"
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "seed-admin-001", RoleId = "1" }
            );

            modelBuilder.Entity<ApplicationUser>()
                .HasQueryFilter(u => !u.IsDeleted);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Address).HasMaxLength(100);

                entity.HasOne(u => u.Cart)
                      .WithOne(c => c.User)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
               
                   entity.HasMany(u => u.RefreshTokens)
                   .WithOne()
                   .OnDelete(DeleteBehavior.Cascade);

            });
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Sellerprofile>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.StoreName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(s => s.StoreName)
                      .IsUnique(); 

                entity.Property(s => s.TotalEarnings)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0);

                entity.Property(s => s.IsApproved)
                      .HasDefaultValue(false);
            });




            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
              .HasOne(r => r.Product)
              .WithMany()
              .HasForeignKey(r => r.ProductId)
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        }
        public List<RefreshToken> RefreshTokens { get; set; } = new();

        public DbSet<Sellerprofile> Sellers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
    }
}
