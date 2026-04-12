using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<CategoryResponse>> GetAll()
    {
        return Ok(_categoryService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CategoryResponse> GetById(Guid id)
    {
        var category = _categoryService.GetById(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CategoryResponse> Create([FromBody] CreateCategoryRequest request)
    {
        var created = _categoryService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
