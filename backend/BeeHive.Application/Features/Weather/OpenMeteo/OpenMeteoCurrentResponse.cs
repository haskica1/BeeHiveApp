using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Weather.OpenMeteo;

/// <summary>Internal shape of the Open-Meteo current-conditions slice.</summary>
internal sealed class OpenMeteoCurrentResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrentData? Current { get; set; }
}

internal sealed class OpenMeteoCurrentData
{
    [JsonPropertyName("temperature_2m")]
    public double? Temperature2m { get; set; }
}
