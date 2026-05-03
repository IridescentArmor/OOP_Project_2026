using Marketplace.API.Data;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Repositories;

public class EfReviewRepository : IReviewRepository
{
    private readonly MarketplaceDbContext _context;

    public EfReviewRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public void Add(Review review)
    {
        _context.Set<Review>().Add(review);
        _context.SaveChanges();
    }

    public void Update(Review review)
    {
        _context.Set<Review>().Update(review);
        _context.SaveChanges();
    }

    public void Delete(Review review)
    {
        _context.Set<Review>().Remove(review);
        _context.SaveChanges();
    }

    public Review? GetById(Guid id) => _context.Set<Review>().FirstOrDefault(r => r.Id == id);

    public IEnumerable<Review> GetAll()
        => _context.Set<Review>()
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToList();

    public IEnumerable<Review> GetByProduct(Guid productId, bool includeHidden = false)
    {
        var q = _context.Set<Review>().AsNoTracking().Where(r => r.ProductId == productId);
        if (!includeHidden)
            q = q.Where(r => !r.IsHidden);
        return q.OrderByDescending(r => r.CreatedAtUtc).ToList();
    }
}

