using Marketplace.API.Data;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly MarketplaceDbContext _context;

    public EfProductRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Product> GetAll() => _context.Products.ToList();

    public Product? GetById(Guid id) => _context.Products.FirstOrDefault(p => p.Id == id);

    public void Add(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "Товар не може бути порожнім");

        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void Update(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "Товар не може бути порожнім");

        _context.Products.Update(product);
        _context.SaveChanges();
    }

    public void Delete(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "Товар не може бути порожнім");

        _context.Products.Remove(product);
        _context.SaveChanges();
    }

    public IEnumerable<Product> GetAllVisibleForGuests()
        => _context.Products
            .Where(p => p.IsActive && !p.IsBlockedByAdmin)
            .ToList();
}
