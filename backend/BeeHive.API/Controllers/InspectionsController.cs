using BeeHive.Application.Features.Inspections;
using BeeHive.Application.Features.Inspections.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BeeHive.API.Controllers;

/// <summary>
/// Records and manages beehive inspections (pregledi). Access to a beehive's inspections is
/// enforced in the service layer (managers within scope, or a Beekeeper assigned to the hive).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _service;
    private readonly IVoiceParsingService _voiceParsingService;
    private readonly IValidator<CreateInspectionDto> _createValidator;
    private readonly IValidator<UpdateInspectionDto> _updateValidator;

    public InspectionsController(
        IInspectionService service,
        IVoiceParsingService voiceParsingService,
        IValidator<CreateInspectionDto> createValidator,
        IValidator<UpdateInspectionDto> updateValidator)
    {
        _service             = service;
        _voiceParsingService = voiceParsingService;
        _createValidator     = createValidator;
        _updateValidator     = updateValidator;
    }

    /// <summary>Returns all inspections for the specified beehive, newest first.</summary>
    [HttpGet("by-beehive/{beehiveId:int}")]
    [ProducesResponseType(typeof(IEnumerable<InspectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBeehive(int beehiveId)
    {
        var inspections = await _service.GetByBeehiveIdAsync(beehiveId);
        return Ok(inspections);
    }

    /// <summary>Returns a single inspection by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var inspection = await _service.GetByIdAsync(id);
        return Ok(inspection);
    }

    /// <summary>Records a new inspection for a beehive the caller can access.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing inspection record.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInspectionDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes an inspection record.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Accepts a recorded audio file, transcribes it, then extracts inspection field values.
    /// Returns transcript + extracted fields (null for anything not mentioned).
    /// </summary>
    [HttpPost("parse-voice")]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting("voice-parse")]
    // Voice notes are short recordings — 15 MB is generous. Without a cap any authenticated
    // user could push the Kestrel default (~30 MB) per request straight to the paid Groq API.
    [RequestSizeLimit(15_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 15_000_000)]
    [ProducesResponseType(typeof(ParseVoiceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ParseVoice(IFormFile? audio)
    {
        if (audio == null || audio.Length == 0)
            return BadRequest(new { message = "Audio file is required." });

        await using var stream = audio.OpenReadStream();
        var result = await _voiceParsingService.ParseAsync(stream, audio.FileName);
        return Ok(result);
    }
}
