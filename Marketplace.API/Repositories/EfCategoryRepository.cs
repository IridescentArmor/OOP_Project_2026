using Marketplace.API.Data;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Repositories;

public class EfCategoryRepository : ICategoryRepository
{
    private readonly MarketplaceDbContext _context;

    public EfCategoryRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Category> GetAll() => _context.Categories.AsNoTracking().ToList();

    public Category? GetById(Guid id) => _context.Categories.FirstOrDefault(c => c.Id == id);

    public void Add(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category), "Категорія не може бути порожньою");

        _context.Categories.Add(category);
        _context.SaveChanges();
    }
}
