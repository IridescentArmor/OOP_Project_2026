using System;
using System.Collections.Generic;
using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product? GetById(Guid id);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    IEnumerable<Product> GetAllVisibleForGuests();
}
