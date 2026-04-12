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

    public IEnumerable<Order> GetAllForUser(Guid userId) =>
        _context.Orders.Where(o => o.UserId == userId).Include(o => o.Items).ToList();
}
