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

    public static readonly Guid DemoApprovedSellerUserId = SeedProductsSellerId;
    public const string DemoApprovedSellerEmail = "seller@market.local";
    public const string DemoApprovedSellerPassword = "Demo123!";

    public static readonly Guid DemoPendingSellerUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public const string DemoPendingSellerEmail = "pending-seller@market.local";
    public const string DemoPendingSellerPassword = "Demo123!";

    public static readonly Guid DemoAdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public const string DemoAdminEmail = "admin@market.local";
    public const string DemoAdminPassword = "Demo123!";
    public static readonly Guid DemoSuperAdminUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public const string DemoSuperAdminEmail = "superadmin@market.local";
    public const string DemoSuperAdminPassword = "Demo123!";

    public static readonly Guid DemoNewSellerUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public const string DemoNewSellerEmail = "new-seller@market.local";
    public const string DemoNewSellerPassword = "Demo123!";

    public static void Seed(MarketplaceDbContext db, IPasswordHasher<User> passwordHasher)
    {
        if (!db.Categories.Any())
        {
            db.Categories.Add(new Category("Електроніка"));
            db.Categories.Add(new Category("Периферія"));
            db.Categories.Add(new Category("Аксесуари"));
            db.Categories.Add(new Category("Дім і побут"));
            db.Categories.Add(new Category("Спорт"));
            db.Categories.Add(new Category("Книги"));
            db.SaveChanges();
        }
        else
        {
            var requiredCategoryNames = new[] { "Електроніка", "Периферія", "Аксесуари", "Дім і побут", "Спорт", "Книги" };
            foreach (var categoryName in requiredCategoryNames)
            {
                if (!db.Categories.Any(c => c.Name == categoryName))
                    db.Categories.Add(new Category(categoryName));
            }
            db.SaveChanges();
        }

        var categoryId = db.Categories.First(c => c.Name == "Електроніка").Id;

        if (!db.Users.Any(u => u.Email == DemoBuyerEmail))
        {
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
        }

        if (!db.Users.Any(u => u.Email == DemoApprovedSellerEmail))
        {
            var seller = User.WithFixedId(
            DemoApprovedSellerUserId,
            "Демо Продавець",
            DemoApprovedSellerEmail,
            "+380111111111",
            "x");
            seller.ChangePasswordHash(passwordHasher.HashPassword(seller, DemoApprovedSellerPassword));
            db.Users.Add(seller);
            db.UserRoleRows.Add(new UserRoleRow
            {
                Id = Guid.NewGuid(),
                UserId = seller.Id,
                RoleType = "Seller",
                SellerCompanyName = "Demo Store",
                SellerRating = 5.0,
                SellerIsApproved = true
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email == DemoPendingSellerEmail))
        {
            var pendingSeller = User.WithFixedId(
            DemoPendingSellerUserId,
            "Очікує модерації",
            DemoPendingSellerEmail,
            "+380222222222",
            "x");
            pendingSeller.ChangePasswordHash(passwordHasher.HashPassword(pendingSeller, DemoPendingSellerPassword));
            db.Users.Add(pendingSeller);
            db.UserRoleRows.Add(new UserRoleRow
            {
                Id = Guid.NewGuid(),
                UserId = pendingSeller.Id,
                RoleType = "Seller",
                SellerCompanyName = "Pending Store",
                SellerRating = 0.0,
                SellerIsApproved = false
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email == DemoAdminEmail))
        {
            var admin = User.WithFixedId(
            DemoAdminUserId,
            "Демо Адмін",
            DemoAdminEmail,
            "+380999999999",
            "x");
            admin.ChangePasswordHash(passwordHasher.HashPassword(admin, DemoAdminPassword));
            db.Users.Add(admin);
            db.UserRoleRows.Add(new UserRoleRow
            {
                Id = Guid.NewGuid(),
                UserId = admin.Id,
                RoleType = "Admin",
                AdminAccessLevel = 10
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email == DemoSuperAdminEmail))
        {
            var superAdmin = User.WithFixedId(
                DemoSuperAdminUserId,
                "Супер Адмін",
                DemoSuperAdminEmail,
                "+380777777777",
                "x");
            superAdmin.ChangePasswordHash(passwordHasher.HashPassword(superAdmin, DemoSuperAdminPassword));
            db.Users.Add(superAdmin);
            db.UserRoleRows.Add(new UserRoleRow
            {
                Id = Guid.NewGuid(),
                UserId = superAdmin.Id,
                RoleType = "Admin",
                AdminAccessLevel = 100
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Email == DemoNewSellerEmail))
        {
            var newSeller = User.WithFixedId(
                DemoNewSellerUserId,
                "Новий Продавець",
                DemoNewSellerEmail,
                "+380333333333",
                "x");
            newSeller.ChangePasswordHash(passwordHasher.HashPassword(newSeller, DemoNewSellerPassword));
            db.Users.Add(newSeller);
            db.UserRoleRows.Add(new UserRoleRow
            {
                Id = Guid.NewGuid(),
                UserId = newSeller.Id,
                RoleType = "Seller",
                SellerCompanyName = "Random Deals UA",
                SellerRating = 4.7,
                SellerIsApproved = true
            });
            db.SaveChanges();
        }

        if (!db.Products.Any())
        {
            var p1 = new Product("Ноутбук ASUS", "Ігровий ноутбук", 1500m, 10, SeedProductsSellerId, categoryId);
            p1.SetImageUrl("https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=1200");
            db.Products.Add(p1);

            var p2 = new Product("Мишка Logitech", "Бездротова мишка", 50m, 100, SeedProductsSellerId, categoryId);
            p2.SetImageUrl("https://images.unsplash.com/photo-1527814050087-3793815479db?w=1200");
            db.Products.Add(p2);

            var p3 = new Product("Клавіатура Keychron", "Механічна", 100m, 50, SeedProductsSellerId, categoryId);
            p3.SetImageUrl("https://images.unsplash.com/photo-1517336714739-489689fd1ca8?w=1200");
            db.Products.Add(p3);

            var randomProducts = new[]
            {
                new { Category = "Дім і побут", Title = "Робот-пилосос Nova", Description = "Розумний робот-пилосос з вологим прибиранням", Price = 289m, Stock = 22, Image = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=1200" },
                new { Category = "Спорт", Title = "Йога-мат ProFlex", Description = "Нещільний мат із протиковзким покриттям", Price = 35m, Stock = 80, Image = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=1200" },
                new { Category = "Книги", Title = "Clean Code (UA)", Description = "Практичний посібник з чистого коду", Price = 19m, Stock = 55, Image = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=1200" },
                new { Category = "Електроніка", Title = "Бездротові навушники AirPulse", Description = "Активне шумозаглушення та 32 години автономності", Price = 129m, Stock = 40, Image = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=1200" },
                new { Category = "Аксесуари", Title = "Рюкзак Urban 20L", Description = "Водовідштовхувальний міський рюкзак", Price = 49m, Stock = 70, Image = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?w=1200" }
            };

            foreach (var item in randomProducts)
            {
                var category = db.Categories.First(c => c.Name == item.Category);
                var product = new Product(item.Title, item.Description, item.Price, item.Stock, DemoNewSellerUserId, category.Id);
                product.SetImageUrl(item.Image);
                db.Products.Add(product);
            }

            db.SaveChanges();
        }
    }
}
