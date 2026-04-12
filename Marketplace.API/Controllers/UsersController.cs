using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UserResponse>> GetAll()
    {
        return Ok(_userService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserResponse> GetById(Guid id)
    {
        var user = _userService.GetById(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<UserResponse> Create([FromBody] CreateUserRequest request)
    {
        var created = _userService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
