using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Inspections.Groq;

internal sealed class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
