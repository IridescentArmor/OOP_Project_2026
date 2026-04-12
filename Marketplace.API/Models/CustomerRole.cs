using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models
{
    public class CustomerRole : IRole
    {
        public UserRoleKind Kind => UserRoleKind.Customer;
        public string RoleName => "Customer";

        public string? DefaultAddress { get; private set; }
        public int LoyaltyPoints { get; private set; }

        private readonly List<Order> _orders = new();
        public IReadOnlyCollection<Order> Orders => _orders;

        public void HydrateFromDatabase(string? defaultAddress, int loyaltyPoints)
        {
            DefaultAddress = defaultAddress;
            LoyaltyPoints = loyaltyPoints;
        }

        public void AttachLoadedOrders(IEnumerable<Order> orders)
        {
            foreach (var o in orders)
                _orders.Add(o);
        }

        public void SetDefaultAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Адреса не може бути порожньою");

            DefaultAddress = address;
        }

        public Order PlaceOrder(Guid userId, Dictionary<Product, int> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
                throw new InvalidOperationException("Кошик порожній");

            var order = new Order(userId);

            foreach (var item in cartItems)
            {
                var product = item.Key;
                var quantity = item.Value;

                if (product == null)
                    throw new ArgumentException("Товар не може бути null");

                if (quantity <= 0)
                    throw new ArgumentException("Кількість має бути більше 0");

                if (product.StockQuantity < quantity)
                    throw new InvalidOperationException($"Недостатньо товару: {product.Title}");

                order.AddItem(product, quantity);
                product.UpdateStock(-quantity);
            }

            _orders.Add(order);

            var total = order.CalculateTotal();
            AddLoyaltyPoints((int)(total / 10));

            return order;
        }

        public bool CancelOrder(Guid orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return false;

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("Замовлення вже відправлено або завершено");

            order.ChangeStatus(OrderStatus.Canceled);
            return true;
        }

        private void AddLoyaltyPoints(int points)
        {
            if (points > 0)
                LoyaltyPoints += points;
        }
    }
}
