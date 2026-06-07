using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Inspections.Groq;

/// <summary>Internal shape of the Groq chat-completions response.</summary>
internal sealed class GroqChatResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}
