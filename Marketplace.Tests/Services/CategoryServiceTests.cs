using Marketplace.API.DTOs;
using Marketplace.API.Repositories;
using Marketplace.API.Services;
using Marketplace.Tests.Helpers;

namespace Marketplace.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public void Create_DuplicateNameCaseInsensitive_ThrowsInvalidOperationException()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var sut = new CategoryService(new EfCategoryRepository(ctx), new EfProductRepository(ctx));

        Assert.Throws<InvalidOperationException>(() =>
            sut.Create(new CreateCategoryRequest { Name = "електроніка" }));
    }

    [Fact]
    public void Create_UniqueName_ReturnsResponse()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var sut = new CategoryService(new EfCategoryRepository(ctx), new EfProductRepository(ctx));

        var res = sut.Create(new CreateCategoryRequest { Name = $"Нова-{Guid.NewGuid():N}" });

        Assert.NotEqual(Guid.Empty, res.Id);
        Assert.False(string.IsNullOrWhiteSpace(res.Name));
    }
}
