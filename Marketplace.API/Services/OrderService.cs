using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class OrderService : IOrderService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public OrderService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public OrderResponse CreateOrder(CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Кошик порожній");

        var user = _userRepository.GetById(request.UserId)
            ?? throw new InvalidOperationException("Користувача не знайдено");

        var customerRole = user.Roles.OfType<CustomerRole>().FirstOrDefault()
            ?? throw new InvalidOperationException("Користувач не має ролі покупця");

        var cart = new Dictionary<Product, int>();
        foreach (var pair in request.Items)
        {
            var product = _productRepository.GetById(pair.Key)
                ?? throw new InvalidOperationException($"Товар не знайдено: {pair.Key}");

            if (pair.Value <= 0)
                throw new ArgumentException("Кількість має бути більше 0");

            cart[product] = pair.Value;
        }

        var order = customerRole.PlaceOrder(user.Id, cart);
        _orderRepository.Add(order);
        _userRepository.UpdateStoredRoles(user);

        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            Total = order.CalculateTotal()
        };
    }
}
