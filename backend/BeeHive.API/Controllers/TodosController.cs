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

    /// <summary>
    /// Creates a new to-do item.
    /// Apiary todos: OrgAdmin and SystemAdmin only.
    /// Hive todos: Admin, OrgAdmin, SystemAdmin, or User assigned to that hive.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var role   = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId = GetUserId();

        if (dto.ApiaryId.HasValue)
        {
            if (role == "OrgAdmin" || role == "SystemAdmin")
            {
                // Allowed — no further scope check needed for these roles
            }
            else if (role == "Admin")
            {
                // Admin may only manage todos for their assigned apiary
                var apiaryIdClaim = User.FindFirstValue("apiaryId");
                if (apiaryIdClaim == null || int.Parse(apiaryIdClaim) != dto.ApiaryId.Value)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
        }
        else if (dto.BeehiveId.HasValue)
        {
            if (role == "User")
            {
                if (userId == null || !await _service.IsUserAssignedToBeehiveAsync(userId.Value, dto.BeehiveId.Value))
                    return Forbid();
            }
            else if (role != "Admin" && role != "OrgAdmin" && role != "SystemAdmin")
            {
                return Forbid();
            }
        }
        else
        {
            return BadRequest(new { message = "Either ApiaryId or BeehiveId must be provided." });
        }

        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a to-do item.
    /// Apiary todos: OrgAdmin and SystemAdmin only.
    /// Hive todos: Admin, OrgAdmin, SystemAdmin, or User assigned to that hive.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var role   = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId = GetUserId();

        var existing = await _service.GetByIdAsync(id);

        if (existing.ApiaryId.HasValue)
        {
            if (role == "OrgAdmin" || role == "SystemAdmin")
            {
                // Allowed
            }
            else if (role == "Admin")
            {
                var apiaryIdClaim = User.FindFirstValue("apiaryId");
                if (apiaryIdClaim == null || int.Parse(apiaryIdClaim) != existing.ApiaryId.Value)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
        }
        else if (existing.BeehiveId.HasValue)
        {
            if (role == "User")
            {
                if (userId == null || !await _service.IsUserAssignedToBeehiveAsync(userId.Value, existing.BeehiveId.Value))
                    return Forbid();
            }
            else if (role != "Admin" && role != "OrgAdmin" && role != "SystemAdmin")
            {
                return Forbid();
            }
        }

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a to-do item.
    /// Apiary todos: OrgAdmin and SystemAdmin only.
    /// Hive todos: Admin, OrgAdmin, SystemAdmin, or User assigned to that hive.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var role   = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId = GetUserId();

        var existing = await _service.GetByIdAsync(id);

        if (existing.ApiaryId.HasValue)
        {
            if (role == "OrgAdmin" || role == "SystemAdmin")
            {
                // Allowed
            }
            else if (role == "Admin")
            {
                var apiaryIdClaim = User.FindFirstValue("apiaryId");
                if (apiaryIdClaim == null || int.Parse(apiaryIdClaim) != existing.ApiaryId.Value)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
        }
        else if (existing.BeehiveId.HasValue)
        {
            if (role == "User")
            {
                if (userId == null || !await _service.IsUserAssignedToBeehiveAsync(userId.Value, existing.BeehiveId.Value))
                    return Forbid();
            }
            else if (role != "Admin" && role != "OrgAdmin" && role != "SystemAdmin")
            {
                return Forbid();
            }
        }

        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Returns users that can be assigned a todo, filtered by the caller's role.</summary>
    [HttpGet("assignable-users")]
    [ProducesResponseType(typeof(IEnumerable<AssignableUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignableUsers()
    {
        var role          = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId        = GetUserId();
        var orgIdClaim    = User.FindFirstValue("organizationId");
        var orgId         = orgIdClaim != null ? int.Parse(orgIdClaim) : (int?)null;
        var apiaryIdClaim = User.FindFirstValue("apiaryId");
        var apiaryId      = apiaryIdClaim != null ? int.Parse(apiaryIdClaim) : (int?)null;

        var users = await _service.GetAssignableUsersAsync(role, userId, orgId, apiaryId);
        return Ok(users);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
