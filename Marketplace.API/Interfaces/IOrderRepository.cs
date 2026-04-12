using System;
using System.Collections.Generic;
using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface IOrderRepository
{
    void Add(Order order);
    Order? GetById(Guid id);
    IEnumerable<Order> GetAllForUser(Guid userId);
}