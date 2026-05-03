using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IOrderService
{
    OrderResponse CreateOrder(CreateOrderRequest request);

    IReadOnlyList<OrderResponse> GetMyOrders(Guid userId);

    void CancelMyOrder(Guid userId, Guid orderId);

    IReadOnlyList<OrderResponse> GetSellerOrders(Guid sellerUserId);

    OrderResponse ChangeStatusAsSeller(Guid sellerUserId, Guid orderId, string newStatus, string? trackingNumber);

    SellerStatsResponse GetSellerStats(Guid sellerUserId, int periodDays);
}
