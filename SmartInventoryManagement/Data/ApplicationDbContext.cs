using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagement.Models;

namespace SmartInventoryManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // ✅ Use IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<GuestOrderView> GuestOrderViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ✅ Ensure Identity setup is called

            modelBuilder.Entity<GuestOrderView>().HasNoKey();

            // ✅ Identity Role Seeding
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
            );

            // ✅ Define Relationship: One Order -> Many OrderItems
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.QuantityInStock)
                .HasDefaultValue(0);

            modelBuilder.Entity<Product>()
                .Property(p => p.LowStockThreshold)
                .HasDefaultValue(1);

            // ✅ Fix PostgreSQL Constraint Syntax
            modelBuilder.Entity<Product>()
                .HasCheckConstraint("CK_Product_QuantityInStock", "\"QuantityInStock\" >= 0");

            modelBuilder.Entity<GuestOrderView>()
                .Property(g => g.Price)
                .HasPrecision(18, 2);

            // ✅ Seed Default Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Devices and gadgets" },
                new Category { Id = 2, Name = "Clothing", Description = "Wearable items" },
                new Category { Id = 3, Name = "Food", Description = "Edible products" }
            );
        }
    }
}
