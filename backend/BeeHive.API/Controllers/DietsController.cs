using System.Security.Claims;
using BeeHive.Application.Features.Beehives;
using BeeHive.Application.Features.Diets;
using BeeHive.Application.Features.Diets.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DietsController : ControllerBase
{
    private readonly IDietService       _service;
    private readonly IBeehiveService    _beehiveService;
    private readonly IValidator<CreateDietDto>      _createValidator;
    private readonly IValidator<UpdateDietDto>      _updateValidator;
    private readonly IValidator<CompleteEarlyDto>   _completeEarlyValidator;

    public DietsController(
        IDietService service,
        IBeehiveService beehiveService,
        IValidator<CreateDietDto> createValidator,
        IValidator<UpdateDietDto> updateValidator,
        IValidator<CompleteEarlyDto> completeEarlyValidator)
    {
        _service                = service;
        _beehiveService         = beehiveService;
        _createValidator        = createValidator;
        _updateValidator        = updateValidator;
        _completeEarlyValidator = completeEarlyValidator;
    }

    /// <summary>Returns all diets for a given beehive, newest first.</summary>
    [HttpGet("by-beehive/{beehiveId:int}")]
    [ProducesResponseType(typeof(IEnumerable<DietDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBeehive(int beehiveId)
    {
        var diets = await _service.GetByBeehiveIdAsync(beehiveId);
        return Ok(diets);
    }

    /// <summary>Returns a single diet with its feeding entries.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DietDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var diet = await _service.GetByIdAsync(id);
        return Ok(diet);
    }

    /// <summary>
    /// Creates a new diet and auto-generates feeding entries.
    /// Admin, OrgAdmin, SystemAdmin: unrestricted within their scope.
    /// User: only for beehives they are assigned to.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DietDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDietDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        if (await IsForbiddenForUserAsync(dto.BeehiveId))
            return Forbid();

        var userId = GetUserId();
        var created = await _service.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a diet (only allowed when not completed/stopped).
    /// Admin, OrgAdmin, SystemAdmin: allowed.
    /// User: only for beehives they are assigned to.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DietDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDietDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var existing = await _service.GetByIdAsync(id);
        if (await IsForbiddenForUserAsync(existing.BeehiveId))
            return Forbid();

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a diet (only allowed before it has started).
    /// Admin, OrgAdmin, SystemAdmin: allowed.
    /// User: only for beehives they are assigned to.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (await IsForbiddenForUserAsync(existing.BeehiveId))
            return Forbid();

        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Stops a diet early.
    /// Admin, OrgAdmin, SystemAdmin: allowed.
    /// User: only for beehives they are assigned to.
    /// </summary>
    [HttpPost("{id:int}/complete-early")]
    [ProducesResponseType(typeof(DietDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteEarly(int id, [FromBody] CompleteEarlyDto dto)
    {
        var validation = await _completeEarlyValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var existing = await _service.GetByIdAsync(id);
        if (await IsForbiddenForUserAsync(existing.BeehiveId))
            return Forbid();

        var updated = await _service.CompleteEarlyAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// Marks a specific feeding entry as completed. Available to all authenticated roles.
    /// </summary>
    [HttpPost("{dietId:int}/feeding-entries/{entryId:int}/complete")]
    [ProducesResponseType(typeof(DietDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteFeedingEntry(int dietId, int entryId)
    {
        await _service.CompleteFeedingEntryAsync(dietId, entryId);
        var updated = await _service.GetByIdAsync(dietId);
        return Ok(updated);
    }

    // Returns true when the caller is a User role who is NOT assigned to the given beehive.
    // Returns false for all other roles (they are always permitted by role).
    private async Task<bool> IsForbiddenForUserAsync(int beehiveId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        if (role != "User") return false;

        var userId = GetUserId();
        if (userId == null) return true;

        return !await _beehiveService.IsUserAssignedToBeehiveAsync(userId.Value, beehiveId);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
