using BeeHive.Application.Features.Inspections.DTOs;

namespace BeeHive.Application.Features.Inspections;

public interface IVoiceParsingService
{
    Task<ParseVoiceResult> ParseAsync(Stream audioStream, string fileName);
}
