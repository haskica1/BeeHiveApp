using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace BeeHive.Application.Features.Inspections;

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IVoiceParsingService
{
    Task<ParseVoiceResult> ParseAsync(string transcript);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class VoiceParsingService : IVoiceParsingService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public VoiceParsingService(HttpClient http, IConfiguration config)
    {
        _http = http;
        var apiKey = config["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey is not configured.");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<ParseVoiceResult> ParseAsync(string transcript)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var prompt = $$$"""
            Si asistent za pčelarstvo. Korisnik je izgovorio sljedeći tekst na bosanskom jeziku:
            "{{{transcript}}}"

            Danas je {{{today}}}.

            Iz teksta izvuci podatke o inspekciji košnice i vrati SAMO validan JSON objekat (bez markdown, bez objašnjenja).
            Koristi isključivo ovaj format:
            {
              "date": "<datum u formatu yyyy-MM-dd, ili null ako nije pomenut>",
              "temperature": <broj s decimalama u Celzijusima, ili null>,
              "honeyLevel": <1 za nizak/malo meda, 2 za srednji/osrednji, 3 za visok/puno meda, ili null>,
              "broodStatus": "<opis legla/matice/jaja, ili null>",
              "notes": "<ostale napomene/zapažanja, ili null>"
            }

            Pravila:
            - Ako je datum relativan ("danas", "juče", "jučer"), pretvori u apsolutni datum u odnosu na danas ({{{today}}}).
            - Ako neka informacija nije pomenuta, postavi to polje na null.
            - honeyLevel: 1=nizak/malo, 2=srednji/osrednji/normalan, 3=visok/puno/odlično.
            - Vrati SAMO JSON, bez ikakvih dodatnih znakova ili teksta.
            """;

        var requestBody = new
        {
            model       = "llama-3.3-70b-versatile",
            temperature = 0.1,
            messages    = new[]
            {
                new { role = "user", content = prompt }
            },
            response_format = new { type = "json_object" },
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.groq.com/openai/v1/chat/completions", requestBody);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<GroqResponse>(JsonOpts)
            ?? throw new InvalidOperationException("Empty response from Groq API.");

        var jsonText = raw.Choices?[0].Message?.Content
            ?? throw new InvalidOperationException("No content in Groq response.");

        var parsed = JsonSerializer.Deserialize<GeminiParsedInspection>(jsonText, JsonOpts)
            ?? throw new InvalidOperationException("Failed to deserialize Groq extraction result.");

        return new ParseVoiceResult
        {
            Date        = parsed.Date,
            Temperature = parsed.Temperature,
            HoneyLevel  = parsed.HoneyLevel.HasValue ? (HoneyLevel)parsed.HoneyLevel.Value : null,
            BroodStatus = parsed.BroodStatus,
            Notes       = parsed.Notes,
        };
    }
}

// ── Internal response shapes ──────────────────────────────────────────────────

internal sealed class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

internal sealed class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}

internal sealed class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

internal sealed class GeminiParsedInspection
{
    [JsonPropertyName("date")]          public string? Date        { get; set; }
    [JsonPropertyName("temperature")]   public double? Temperature { get; set; }
    [JsonPropertyName("honeyLevel")]    public int?    HoneyLevel  { get; set; }
    [JsonPropertyName("broodStatus")]   public string? BroodStatus { get; set; }
    [JsonPropertyName("notes")]         public string? Notes       { get; set; }
}
