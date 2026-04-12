using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models
{
    public class Category : IEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Name { get; private set; } = default!;

        private readonly List<Product> _products = new();

        protected Category() { }

        public IReadOnlyCollection<Product> Products => _products;

        public Category(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Назва не може бути порожньою", nameof(name));
            }

            Name = name;
        }

        public void AddProduct(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            if (!_products.Contains(product))
            {
                _products.Add(product);
            }
        }

        public bool RemoveProduct(Guid productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return false;

            _products.Remove(product);
            return true;
        }
    }
}
