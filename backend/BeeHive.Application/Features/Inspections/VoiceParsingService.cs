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
    Task<ParseVoiceResult> ParseAsync(Stream audioStream, string fileName);
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

    public async Task<ParseVoiceResult> ParseAsync(Stream audioStream, string fileName)
    {
        var transcript = await TranscribeAsync(audioStream, fileName);
        var fields     = await ExtractFieldsAsync(transcript);
        fields.Transcript = transcript;
        return fields;
    }

    // ── Step 1: Whisper transcription ─────────────────────────────────────────

    private async Task<string> TranscribeAsync(Stream audioStream, string fileName)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(audioStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent("whisper-large-v3-turbo"), "model");
        content.Add(new StringContent("bs"), "language");
        content.Add(new StringContent("text"), "response_format");

        var response = await _http.PostAsync(
            "https://api.groq.com/openai/v1/audio/transcriptions", content);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadAsStringAsync()).Trim();
    }

    // ── Step 2: Llama field extraction ────────────────────────────────────────

    private async Task<ParseVoiceResult> ExtractFieldsAsync(string transcript)
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
            model           = "llama-3.3-70b-versatile",
            temperature     = 0.1,
            messages        = new[] { new { role = "user", content = prompt } },
            response_format = new { type = "json_object" },
        };

        var response = await _http.PostAsJsonAsync(
            "https://api.groq.com/openai/v1/chat/completions", requestBody);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOpts)
            ?? throw new InvalidOperationException("Empty response from Groq chat API.");

        var jsonText = raw.Choices?[0].Message?.Content
            ?? throw new InvalidOperationException("No content in Groq chat response.");

        var parsed = JsonSerializer.Deserialize<GroqParsedInspection>(jsonText, JsonOpts)
            ?? throw new InvalidOperationException("Failed to deserialize field extraction result.");

        return new ParseVoiceResult
        {
            Date        = parsed.Date,
            Temperature = parsed.Temperature,
            HoneyLevel  = parsed.HoneyLevel.HasValue ? (HoneyLevel)parsed.HoneyLevel.Value : null,
            BroodStatus = parsed.BroodStatus,
            Notes       = parsed.Notes,
        };
    }

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".webm" => "audio/webm",
            ".mp4"  => "audio/mp4",
            ".m4a"  => "audio/mp4",
            ".ogg"  => "audio/ogg",
            ".wav"  => "audio/wav",
            ".mp3"  => "audio/mpeg",
            _       => "audio/webm",
        };
}

// ── Internal response shapes ──────────────────────────────────────────────────

internal sealed class GroqChatResponse
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

internal sealed class GroqParsedInspection
{
    [JsonPropertyName("date")]          public string? Date        { get; set; }
    [JsonPropertyName("temperature")]   public double? Temperature { get; set; }
    [JsonPropertyName("honeyLevel")]    public int?    HoneyLevel  { get; set; }
    [JsonPropertyName("broodStatus")]   public string? BroodStatus { get; set; }
    [JsonPropertyName("notes")]         public string? Notes       { get; set; }
}
