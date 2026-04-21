using BeeHive.Application.Features.Beehives;
using BeeHive.Application.Features.Beehives.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class BeehivesController : ControllerBase
{
    private readonly IBeehiveService _service;
    private readonly IValidator<CreateBeehiveDto> _createValidator;
    private readonly IValidator<UpdateBeehiveDto> _updateValidator;

    public BeehivesController(
        IBeehiveService service,
        IValidator<CreateBeehiveDto> createValidator,
        IValidator<UpdateBeehiveDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Returns all beehives belonging to the specified apiary.</summary>
    [HttpGet("by-apiary/{apiaryId:int}")]
    [ProducesResponseType(typeof(IEnumerable<BeehiveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByApiary(int apiaryId)
    {
        var beehives = await _service.GetByApiaryIdAsync(apiaryId);
        return Ok(beehives);
    }

    /// <summary>Returns a single beehive including all its inspections.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BeehiveDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var beehive = await _service.GetByIdAsync(id);
        return Ok(beehive);
    }

    /// <summary>Creates a new beehive within an apiary.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BeehiveDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBeehiveDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing beehive.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BeehiveDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBeehiveDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes a beehive and all its inspections.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
