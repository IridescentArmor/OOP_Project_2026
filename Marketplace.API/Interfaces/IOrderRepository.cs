using System;
using System.Collections.Generic;
using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface IOrderRepository
{
    void Add(Order order);
    Order? GetById(Guid id);
    IEnumerable<Order> GetAll();
    IEnumerable<Order> GetAllForUser(Guid userId);
    IEnumerable<Order> GetAllForSeller(Guid sellerId);

    void Update(Order order);
    void Delete(Order order);

    bool HasActiveOrdersForProduct(Guid productId);

    bool HasActiveOrdersForSeller(Guid sellerId);
}