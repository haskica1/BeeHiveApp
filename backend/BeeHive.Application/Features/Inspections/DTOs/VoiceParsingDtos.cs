using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Inspections.DTOs;

public class ParseVoiceRequest
{
    public string Transcript { get; set; } = string.Empty;
}

public class ParseVoiceResult
{
    public string? Date { get; set; }
    public double? Temperature { get; set; }
    public HoneyLevel? HoneyLevel { get; set; }
    public string? BroodStatus { get; set; }
    public string? Notes { get; set; }
}
