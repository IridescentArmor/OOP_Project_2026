using Marketplace.API.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Marketplace.API.Models;

namespace Marketplace.Tests.Helpers;

public static class TestDatabase
{
    public static (MarketplaceDbContext Context, string DbPath) CreateContext(bool runSeed = false)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"unit_{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        var context = new MarketplaceDbContext(options);
        context.Database.Migrate();

        if (runSeed)
            DbInitializer.Seed(context, new PasswordHasher<User>());

        return (context, dbPath);
    }
}
