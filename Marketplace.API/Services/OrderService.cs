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
        order.SetShippingInfo(request.RecipientName, request.RecipientPhone, request.ShippingAddress);
        _orderRepository.Add(order);
        _userRepository.UpdateStoredRoles(user);

        return ToResponse(order);
    }

    public IReadOnlyList<OrderResponse> GetMyOrders(Guid userId)
        => _orderRepository.GetAllForUser(userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(ToResponse)
            .ToList();

    public void CancelMyOrder(Guid userId, Guid orderId)
    {
        var order = _orderRepository.GetById(orderId)
            ?? throw new InvalidOperationException("Замовлення не знайдено");

        if (order.UserId != userId)
            throw new InvalidOperationException("Немає доступу до цього замовлення");

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Замовлення вже відправлено або завершено");

        if (order.Status == OrderStatus.Canceled)
            return;

        RaiseStatusChangeWithAudit(order, OrderStatus.Canceled);
        _orderRepository.Update(order);
    }

    public IReadOnlyList<OrderResponse> GetSellerOrders(Guid sellerUserId)
        => _orderRepository.GetAllForSeller(sellerUserId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(ToResponse)
            .ToList();

    public OrderResponse ChangeStatusAsSeller(Guid sellerUserId, Guid orderId, string newStatus, string? trackingNumber)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, ignoreCase: true, out var parsed))
            throw new InvalidOperationException("Невідомий статус замовлення");

        var order = _orderRepository.GetById(orderId)
            ?? throw new InvalidOperationException("Замовлення не знайдено");

        if (order.SellerId != sellerUserId)
            throw new InvalidOperationException("Немає доступу до цього замовлення");

        RaiseStatusChangeWithAudit(order, parsed, trackingNumber);
        _orderRepository.Update(order);
        return ToResponse(order);
    }

    public SellerStatsResponse GetSellerStats(Guid sellerUserId, int periodDays)
    {
        if (periodDays is not (7 or 30))
            throw new InvalidOperationException("Період статистики має бути 7 або 30 днів");

        var fromUtc = DateTime.UtcNow.Date.AddDays(-(periodDays - 1));
        var completed = _orderRepository.GetAllForSeller(sellerUserId)
            .Where(o => o.Status == OrderStatus.Completed)
            .Where(o => o.CreatedAtUtc >= fromUtc)
            .ToList();

        var grouped = completed
            .GroupBy(o => o.CreatedAtUtc.Date)
            .ToDictionary(
                g => g.Key,
                g => new SellerStatsPointResponse
                {
                    Label = g.Key.ToString("dd.MM"),
                    Revenue = g.Sum(o => o.CalculateTotal()),
                    OrdersCount = g.Count()
                });

        var points = Enumerable.Range(0, periodDays)
            .Select(offset =>
            {
                var date = fromUtc.AddDays(offset);
                return grouped.TryGetValue(date, out var point)
                    ? point
                    : new SellerStatsPointResponse
                    {
                        Label = date.ToString("dd.MM"),
                        Revenue = 0,
                        OrdersCount = 0
                    };
            })
            .ToList();

        return new SellerStatsResponse
        {
            PeriodDays = periodDays,
            CompletedOrdersCount = completed.Count,
            Revenue = completed.Sum(o => o.CalculateTotal()),
            SoldItemsCount = completed.Sum(o => o.Items.Sum(i => i.Quantity)),
            Points = points
        };
    }

    private static void RaiseStatusChangeWithAudit(Order order, OrderStatus newStatus, string? trackingNumber = null)
    {
        order.StatusChanged += OrderStatusAudit.RecordTransition;
        try
        {
            order.ChangeStatus(newStatus, trackingNumber);
        }
        finally
        {
            order.StatusChanged -= OrderStatusAudit.RecordTransition;
        }
    }

    private OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        SellerId = order.SellerId,
        Status = order.Status.ToString(),
        Total = order.CalculateTotal(),
        TrackingNumber = order.TrackingNumber,
        CreatedAtUtc = order.CreatedAtUtc,
        RecipientName = order.RecipientName,
        RecipientPhone = order.RecipientPhone,
        ShippingAddress = order.ShippingAddress,
        Items = order.Items.Select(i =>
        {
            var product = _productRepository.GetById(i.ProductId);
            return new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductTitle = product?.Title ?? "(невідомий товар)",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.CalculateSubtotal()
            };
        }).ToList()
    };
}
