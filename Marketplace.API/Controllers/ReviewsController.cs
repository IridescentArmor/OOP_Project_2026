using System.Security.Claims;
using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ReviewResponse>> GetAll(Guid productId)
    {
        var includeHidden = User.IsInRole("Admin");
        return Ok(_reviewService.GetByProduct(productId, includeHidden));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    public ActionResult<ReviewResponse> Create(Guid productId, [FromBody] CreateReviewRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return Ok(_reviewService.Create(userId, productId, request));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{reviewId:guid}/hide")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Hide(Guid reviewId, [FromBody] ModerateReviewRequest request)
    {
        _reviewService.Hide(reviewId, request.Reason);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{reviewId:guid}/unhide")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Unhide(Guid reviewId)
    {
        _reviewService.Unhide(reviewId);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Delete(Guid reviewId, [FromBody] ModerateReviewRequest request)
    {
        _reviewService.Delete(reviewId, request.Reason);
        return NoContent();
    }
}

