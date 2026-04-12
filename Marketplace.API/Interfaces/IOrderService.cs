using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IOrderService
{
    OrderResponse CreateOrder(CreateOrderRequest request);
}
