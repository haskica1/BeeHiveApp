using BeeHive.Application.Features.Inspections;
using BeeHive.Application.Features.Inspections.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _service;
    private readonly IValidator<CreateInspectionDto> _createValidator;
    private readonly IValidator<UpdateInspectionDto> _updateValidator;

    public InspectionsController(
        IInspectionService service,
        IValidator<CreateInspectionDto> createValidator,
        IValidator<UpdateInspectionDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Returns all inspections for the specified beehive, newest first.</summary>
    [HttpGet("by-beehive/{beehiveId:int}")]
    [ProducesResponseType(typeof(IEnumerable<InspectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBeehive(int beehiveId)
    {
        var inspections = await _service.GetByBeehiveIdAsync(beehiveId);
        return Ok(inspections);
    }

    /// <summary>Returns a single inspection by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var inspection = await _service.GetByIdAsync(id);
        return Ok(inspection);
    }

    /// <summary>Records a new inspection for a beehive.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing inspection record. Not available to User role.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,OrgAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInspectionDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes an inspection record. Not available to User role.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,OrgAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
