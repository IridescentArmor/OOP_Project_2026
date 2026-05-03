using System.Security.Claims;
using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Marketplace.API.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private readonly IReviewService _reviewService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public AdminController(
        IUserRepository userRepository,
        IUserService userService,
        IReviewService reviewService,
        IOrderRepository orderRepository,
        IProductService productService,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _userRepository = userRepository;
        _userService = userService;
        _reviewService = reviewService;
        _orderRepository = orderRepository;
        _productService = productService;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpPost("sellers/{userId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ApproveSeller(Guid userId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level))
            return Forbid();

        if (level < 10)
            return Forbid();

        var user = _userRepository.GetById(userId);
        if (user == null)
            return NotFound();

        var seller = user.Roles.OfType<SellerRole>().FirstOrDefault();
        if (seller == null)
            return BadRequest("Користувач не має ролі продавця");

        if (!seller.IsApproved)
        {
            seller.Approve();
            _userRepository.UpdateStoredRoles(user);
        }

        return NoContent();
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UserResponse>> GetUsers()
    {
        return Ok(_userService.GetAll());
    }

    [HttpGet("reviews")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminReviewResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminReviewResponse>> GetReviews()
    {
        return Ok(_reviewService.GetAllForAdmin());
    }

    [HttpGet("orders")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminOrderResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminOrderResponse>> GetOrders()
    {
        var users = _userRepository.GetAll().ToDictionary(u => u.Id, u => u.Name);
        var orders = _orderRepository.GetAll()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                Status = o.Status.ToString(),
                Total = o.CalculateTotal(),
                CreatedAtUtc = o.CreatedAtUtc,
                CustomerName = users.TryGetValue(o.UserId, out var customerName) ? customerName : o.UserId.ToString(),
                SellerName = users.TryGetValue(o.SellerId, out var sellerName) ? sellerName : o.SellerId.ToString(),
                RecipientName = o.RecipientName,
                RecipientPhone = o.RecipientPhone,
                ShippingAddress = o.ShippingAddress,
                TrackingNumber = o.TrackingNumber
            })
            .ToList();

        return Ok(orders);
    }

    [HttpDelete("orders/{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteOrder(Guid orderId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var order = _orderRepository.GetById(orderId);
        if (order == null)
            return NotFound();

        _orderRepository.Delete(order);
        return NoContent();
    }

    [HttpGet("products")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminProductResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminProductResponse>> GetProducts()
    {
        var users = _userRepository.GetAll().ToDictionary(u => u.Id, u => u.Name);
        var products = _productService.GetAllForAdmin()
            .Select(p => new AdminProductResponse
            {
                Product = p,
                SellerName = users.TryGetValue(p.SellerId, out var sellerName) ? sellerName : p.SellerId.ToString()
            })
            .ToList();

        return Ok(products);
    }

    [HttpPut("products/{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ProductResponse> UpdateProduct(Guid productId, [FromBody] CreateProductRequest request)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        return Ok(_productService.UpdateAsAdmin(productId, request));
    }

    [HttpDelete("products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult DeleteProduct(Guid productId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        _productService.DeleteAsAdmin(productId);
        return NoContent();
    }

    [HttpGet("products/export-json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ExportProductsJson()
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var categoriesById = _categoryRepository.GetAll().ToDictionary(c => c.Id, c => c.Name);
        var rows = _productRepository.GetAll()
            .Select(p => new ProductJsonRecord
            {
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                SellerId = p.SellerId,
                CategoryName = categoriesById.TryGetValue(p.CategoryId, out var categoryName) ? categoryName : "Без категорії",
                ImageUrl = p.ImageUrl
            })
            .ToList();

        var json = ProductCatalogJsonSerializer.SerializeRecords(rows);
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "products-export.json");
    }

    [HttpPost("products/import-json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportProductsJson([FromForm] IFormFile file)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest("Файл JSON не вибрано");

        List<ProductJsonRecord>? records;
        await using (var stream = file.OpenReadStream())
        {
            records = await ProductCatalogJsonSerializer.DeserializeRecordsAsync(stream, HttpContext.RequestAborted);
        }

        if (records == null || records.Count == 0)
            return BadRequest("У JSON немає записів для імпорту");

        var categoriesByName = _categoryRepository.GetAll()
            .ToDictionary(c => c.Name.Trim(), c => c, StringComparer.OrdinalIgnoreCase);

        var importedCount = 0;
        foreach (var item in records)
        {
            if (string.IsNullOrWhiteSpace(item.Title) || item.Price <= 0 || item.StockQuantity < 0 || item.SellerId == Guid.Empty)
                continue;

            var categoryName = string.IsNullOrWhiteSpace(item.CategoryName) ? "Без категорії" : item.CategoryName.Trim();
            if (!categoriesByName.TryGetValue(categoryName, out var category))
            {
                category = new Category(categoryName);
                _categoryRepository.Add(category);
                categoriesByName[categoryName] = category;
            }

            var product = new Product(
                item.Title.Trim(),
                item.Description ?? string.Empty,
                item.Price,
                item.StockQuantity,
                item.SellerId,
                category.Id);
            product.SetImageUrl(item.ImageUrl);
            _productRepository.Add(product);
            importedCount++;
        }

        return Ok(new { imported = importedCount, total = records.Count });
    }

    [HttpPut("users/{userId:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> UpdateUser(Guid userId, [FromBody] AdminUpdateUserRequest request)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var requesterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(requesterIdClaim) || !Guid.TryParse(requesterIdClaim, out var requesterId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target) && requesterId != userId)
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        if (target.Roles.OfType<AdminRole>().Any() && level < 100)
            return Forbid();

        return Ok(_userService.UpdateByAdmin(userId, request));
    }

    [HttpPost("users/{userId:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult BlockUser(Guid userId, [FromBody] BlockUserRequest request)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var requesterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(requesterIdClaim) || !Guid.TryParse(requesterIdClaim, out var requesterId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target) && requesterId != userId)
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        if (target.Roles.OfType<AdminRole>().Any() && level < 100)
            return Forbid();

        if (target.Roles.OfType<SellerRole>().Any() && _orderRepository.HasActiveOrdersForSeller(target.Id))
            return BadRequest("Не можна блокувати продавця з активними замовленнями");

        var requester = _userRepository.GetById(requesterId);
        if (requester == null)
            return Unauthorized();

        var actingAdmin = requester.GetRole<AdminRole>();
        if (actingAdmin == null)
            return Forbid();

        try
        {
            actingAdmin.BlockUser(target, request.Reason);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        _userRepository.Update(target);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UnblockUser(Guid userId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var requesterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(requesterIdClaim) || !Guid.TryParse(requesterIdClaim, out var requesterId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target) && requesterId != userId)
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        if (target.Roles.OfType<AdminRole>().Any() && level < 100)
            return Forbid();

        var requester = _userRepository.GetById(requesterId);
        if (requester == null)
            return Unauthorized();

        var actingAdmin = requester.GetRole<AdminRole>();
        if (actingAdmin == null)
            return Forbid();

        try
        {
            actingAdmin.UnblockUser(target);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        _userRepository.Update(target);
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(Guid userId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 10)
            return Forbid();

        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminUserId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target) && adminUserId != userId)
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        _userService.DeleteByAdmin(adminUserId, level, userId);
        return NoContent();
    }

    [HttpPost("users/{userId:guid}/grant-admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> GrantAdminRole(Guid userId, [FromBody] GrantAdminRoleRequest request)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 100)
            return Forbid();

        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminUserId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target) && adminUserId != userId)
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        var updated = _userService.GrantAdminRoleBySuperAdmin(adminUserId, userId, request.AccessLevel);
        return Ok(updated);
    }

    [HttpPost("users/{userId:guid}/revoke-admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> RevokeAdminRole(Guid userId)
    {
        var levelClaim = User.FindFirstValue("admin_access_level");
        if (string.IsNullOrWhiteSpace(levelClaim) || !int.TryParse(levelClaim, out var level) || level < 100)
            return Forbid();

        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminUserId))
            return Unauthorized();

        var target = _userRepository.GetById(userId);
        if (target == null)
            return NotFound();

        if (IsSuperAdmin(target))
            return BadRequest("Змінювати супер-адміністратора може лише він сам");

        var updated = _userService.RevokeAdminRoleBySuperAdmin(adminUserId, userId);
        return Ok(updated);
    }

    private static bool IsSuperAdmin(User user)
        => user.GetRole<AdminRole>()?.AccessLevel >= 100;
}

