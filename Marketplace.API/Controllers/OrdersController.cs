using System.Security.Claims;
using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<OrderResponse> Create([FromBody] CreateOrderRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        request.UserId = userId;
        var result = _orderService.CreateOrder(request);
        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OrderResponse>> GetMy()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return Ok(_orderService.GetMyOrders(userId));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Cancel(Guid orderId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        _orderService.CancelMyOrder(userId, orderId);
        return NoContent();
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("seller")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OrderResponse>> GetSellerOrders()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var sellerUserId))
            return Unauthorized();

        return Ok(_orderService.GetSellerOrders(sellerUserId));
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("seller/stats")]
    [ProducesResponseType(typeof(SellerStatsResponse), StatusCodes.Status200OK)]
    public ActionResult<SellerStatsResponse> GetSellerStats([FromQuery] int periodDays = 7)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var sellerUserId))
            return Unauthorized();

        return Ok(_orderService.GetSellerStats(sellerUserId, periodDays));
    }

    [Authorize(Roles = "Seller")]
    [HttpPost("{orderId:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> ChangeStatus(Guid orderId, [FromBody] ChangeOrderStatusRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var sellerUserId))
            return Unauthorized();

        var result = _orderService.ChangeStatusAsSeller(sellerUserId, orderId, request.Status, request.TrackingNumber);
        return Ok(result);
    }
}
