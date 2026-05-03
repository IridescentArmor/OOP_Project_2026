using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Data;

public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRoleRow> UserRoleRows => Set<UserRoleRow>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Ignore(u => u.Roles);
            e.Property(u => u.IsBlocked).HasDefaultValue(false);
            e.Property(u => u.BlockReason);
            e.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
            e.Property(u => u.LockoutUntilUtc);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired();
            e.Property(c => c.ParentCategoryId);
            e.HasIndex(c => c.Name).IsUnique();
            e.Ignore(c => c.Products);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).IsRequired();
            e.Property(p => p.Description).IsRequired();
            e.Property(p => p.ImageUrl).HasDefaultValue(string.Empty);
            e.Property(p => p.IsActive).HasDefaultValue(true);
            e.Property(p => p.IsBlockedByAdmin).HasDefaultValue(false);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.TrackingNumber);
            e.Property(o => o.CreatedAtUtc);
            e.Property(o => o.RecipientName).IsRequired();
            e.Property(o => o.RecipientPhone).IsRequired();
            e.Property(o => o.ShippingAddress).IsRequired();
            e.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
        });

        modelBuilder.Entity<UserRoleRow>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.UserId);
            e.Property(r => r.RoleType).IsRequired();
            e.Property(r => r.SellerIsApproved).HasDefaultValue(false);
            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Text).IsRequired();
            e.Property(r => r.Rating).IsRequired();
            e.Property(r => r.IsHidden).HasDefaultValue(false);
            e.Property(r => r.CreatedAtUtc);
            e.HasIndex(r => r.ProductId);
        });
    }
}
