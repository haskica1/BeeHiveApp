using System.Net.Http.Json;
using System.Text;
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
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public VoiceParsingService(HttpClient http, IConfiguration config)
    {
        _http   = http;
        _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
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
            {{
              "date": "<datum u formatu yyyy-MM-dd, ili null ako nije pomenut>",
              "temperature": <broj s decimalama u Celzijusima, ili null>,
              "honeyLevel": <1 za nizak/malo meda, 2 za srednji/osrednji, 3 za visok/puno meda, ili null>,
              "broodStatus": "<opis legla/matice/jaja, ili null>",
              "notes": "<ostale napomene/zapažanja, ili null>"
            }}

            Pravila:
            - Ako je datum relativan ("danas", "juče", "jučer"), pretvori u apsolutni datum u odnosu na danas ({{{today}}}).
            - Ako neka informacija nije pomenuta, postavi to polje na null.
            - honeyLevel: 1=nizak/malo, 2=srednji/osrednji/normalan, 3=visok/puno/odlično.
            - Vrati SAMO JSON, bez ikakvih dodatnih znakova ili teksta.
            """;

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature      = 0.1,
                responseMimeType = "application/json",
            }
        };

        var url      = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var response = await _http.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOpts)
            ?? throw new InvalidOperationException("Empty response from Gemini API.");

        var jsonText = raw.Candidates?[0].Content?.Parts?[0].Text
            ?? throw new InvalidOperationException("No content in Gemini response.");

        // Strip possible markdown fences that the model may include despite instructions
        jsonText = jsonText.Trim();
        if (jsonText.StartsWith("```"))
        {
            var start = jsonText.IndexOf('\n') + 1;
            var end   = jsonText.LastIndexOf("```");
            if (end > start) jsonText = jsonText[start..end].Trim();
        }

        var parsed = JsonSerializer.Deserialize<GeminiParsedInspection>(jsonText, JsonOpts)
            ?? throw new InvalidOperationException("Failed to deserialize Gemini extraction result.");

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

internal sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

internal sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}

internal sealed class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }
}

internal sealed class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

internal sealed class GeminiParsedInspection
{
    [JsonPropertyName("date")]          public string?  Date        { get; set; }
    [JsonPropertyName("temperature")]   public double?  Temperature { get; set; }
    [JsonPropertyName("honeyLevel")]    public int?     HoneyLevel  { get; set; }
    [JsonPropertyName("broodStatus")]   public string?  BroodStatus { get; set; }
    [JsonPropertyName("notes")]         public string?  Notes       { get; set; }
}
