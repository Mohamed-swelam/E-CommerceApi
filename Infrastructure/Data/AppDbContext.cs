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

            // ApplicationUser

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasQueryFilter(u => !u.IsDeleted);

                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(u => u.Address)
                      .HasMaxLength(100);

                // User -> Cart
                entity.HasOne(u => u.Cart)
                      .WithOne(c => c.User)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // User -> RefreshTokens
                entity.HasMany(u => u.RefreshTokens)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // User -> SellerProfile
                entity.HasOne(u => u.Seller)
                      .WithOne(s => s.User)
                      .HasForeignKey<Sellerprofile>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // User -> Reviews
                entity.HasMany(u => u.Reviews)
                      .WithOne(r => r.User)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SellerProfile

            modelBuilder.Entity<Sellerprofile>(entity =>
            {
                entity.Property(s => s.StoreName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(s => s.StoreName)
                      .IsUnique();

                entity.Property(s => s.TotalEarnings)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0);

                entity.Property(s => s.IsApproved)
                      .HasDefaultValue(false);

                // Seller -> Products
                entity.HasMany(s => s.Products)
                      .WithOne(p => p.SellerProfile)
                      .HasForeignKey(p => p.SellerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Category

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasMany(c => c.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Product

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Description)
                      .HasMaxLength(1000);

                entity.Property(p => p.Price)
                      .HasPrecision(18, 2);

                entity.HasMany(p => p.ImagesNames)
                      .WithOne(pi => pi.Product)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Reviews)
                      .WithOne(r => r.Product)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ProductImage

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(pi => pi.ImageName)
                      .IsRequired()
                      .HasMaxLength(255);
            });

            // Review

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(r => r.Comment)
                      .HasMaxLength(1000);
            });

            // CartItem

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderItem

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // WishlistItem

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // OrderItem

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            // Payment

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
