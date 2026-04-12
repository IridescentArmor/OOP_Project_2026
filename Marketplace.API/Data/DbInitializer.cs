using Marketplace.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Data;

public static class DbInitializer
{
    public static readonly Guid DemoBuyerUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string DemoBuyerEmail = "buyer@market.local";
    public const string DemoBuyerPassword = "Demo123!";

    public static readonly Guid SeedProductsSellerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static void Seed(MarketplaceDbContext db, IPasswordHasher<User> passwordHasher)
    {
        if (db.Categories.Any())
            return;

        db.Categories.Add(new Category("Електроніка"));
        db.Categories.Add(new Category("Периферія"));
        db.Categories.Add(new Category("Аксесуари"));
        db.SaveChanges();

        var categoryId = db.Categories.First(c => c.Name == "Електроніка").Id;

        var buyer = User.WithFixedId(
            DemoBuyerUserId,
            "Демо Покупець",
            DemoBuyerEmail,
            "+380000000000",
            "x"
            );
        var hash = passwordHasher.HashPassword(buyer, DemoBuyerPassword);
        buyer.ChangePasswordHash(hash);
        db.Users.Add(buyer);
        db.UserRoleRows.Add(new UserRoleRow
        {
            Id = Guid.NewGuid(),
            UserId = buyer.Id,
            RoleType = "Customer",
            CustomerLoyaltyPoints = 0
        });
        db.SaveChanges();

        db.Products.Add(new Product("Ноутбук ASUS", "Ігровий ноутбук", 1500m, 10, SeedProductsSellerId, categoryId));
        db.Products.Add(new Product("Мишка Logitech", "Бездротова мишка", 50m, 100, SeedProductsSellerId, categoryId));
        db.Products.Add(new Product("Клавіатура Keychron", "Механічна", 100m, 50, SeedProductsSellerId, categoryId));
        db.SaveChanges();
    }
}
