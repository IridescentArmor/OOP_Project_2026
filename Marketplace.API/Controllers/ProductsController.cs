using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductResponse>> GetAll()
    {
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        var product = _productService.GetById(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProductResponse> Create([FromBody] CreateProductRequest request)
    {
        var created = _productService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
