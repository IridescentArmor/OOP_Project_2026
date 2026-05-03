using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models
{
    public class Category : IEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string Name { get; private set; } = default!;
        public Guid? ParentCategoryId { get; private set; }

        private readonly List<Product> _products = new();

        protected Category() { }

        public IReadOnlyCollection<Product> Products => _products;

        public Category(string name, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Назва не може бути порожньою", nameof(name));
            }

            Name = name.Trim();
            ParentCategoryId = parentCategoryId;
        }

        public void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва не може бути порожньою", nameof(name));

            Name = name.Trim();
        }

        public void ChangeParent(Guid? parentCategoryId)
        {
            if (parentCategoryId == Id)
                throw new ArgumentException("Категорія не може бути вкладена сама в себе", nameof(parentCategoryId));

            ParentCategoryId = parentCategoryId;
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
