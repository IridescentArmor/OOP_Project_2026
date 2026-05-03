using Marketplace.API.Data;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Repositories;

public class EfOrderRepository : IOrderRepository
{
    private readonly MarketplaceDbContext _context;

    public EfOrderRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public void Add(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order), "Замовлення не може бути порожнім");

        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order? GetById(Guid id) =>
        _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);

    public IEnumerable<Order> GetAll()
        => _context.Orders.Include(o => o.Items).ToList();

    public IEnumerable<Order> GetAllForUser(Guid userId) =>
        _context.Orders.Where(o => o.UserId == userId).Include(o => o.Items).ToList();

    public IEnumerable<Order> GetAllForSeller(Guid sellerId) =>
        _context.Orders.Where(o => o.SellerId == sellerId).Include(o => o.Items).ToList();

    public void Update(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order), "Замовлення не може бути порожнім");

        _context.Orders.Update(order);
        _context.SaveChanges();
    }

    public void Delete(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order), "Замовлення не може бути порожнім");

        _context.Orders.Remove(order);
        _context.SaveChanges();
    }

    public bool HasActiveOrdersForProduct(Guid productId)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Any(o =>
                o.Status != OrderStatus.Completed &&
                o.Status != OrderStatus.Canceled &&
                o.Items.Any(i => i.ProductId == productId));
    }

    public bool HasActiveOrdersForSeller(Guid sellerId)
    {
        return _context.Orders
            .AsNoTracking()
            .Any(o =>
                o.SellerId == sellerId &&
                o.Status != OrderStatus.Completed &&
                o.Status != OrderStatus.Canceled);
    }
}
