using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Inspections.Groq;

internal sealed class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}
