using System.Security.Claims;
using BeeHive.Application.Features.Todos;
using BeeHive.Application.Features.Todos.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _service;
    private readonly IValidator<CreateTodoDto> _createValidator;
    private readonly IValidator<UpdateTodoDto> _updateValidator;

    public TodosController(
        ITodoService service,
        IValidator<CreateTodoDto> createValidator,
        IValidator<UpdateTodoDto> updateValidator)
    {
        _service         = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Returns all to-do items for the given apiary, open items first.</summary>
    [HttpGet("by-apiary/{apiaryId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TodoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByApiary(int apiaryId)
    {
        var todos = await _service.GetByApiaryIdAsync(apiaryId);
        return Ok(todos);
    }

    /// <summary>Returns all to-do items for the given beehive, open items first.</summary>
    [HttpGet("by-beehive/{beehiveId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TodoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBeehive(int beehiveId)
    {
        var todos = await _service.GetByBeehiveIdAsync(beehiveId);
        return Ok(todos);
    }

    /// <summary>Returns a single to-do item by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _service.GetByIdAsync(id);
        return Ok(todo);
    }

    /// <summary>Creates a new to-do item. Available to all authenticated roles.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var userId = GetUserId();
        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a to-do item. Available to all authenticated roles.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes a to-do item. Not available to User role.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,OrgAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
