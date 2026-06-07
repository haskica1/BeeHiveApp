using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Weather.OpenMeteo;

/// <summary>Internal shape of the Open-Meteo forecast response.</summary>
internal sealed class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]  public double  Latitude  { get; set; }
    [JsonPropertyName("longitude")] public double  Longitude { get; set; }
    [JsonPropertyName("timezone")]  public string? Timezone  { get; set; }
    [JsonPropertyName("daily")]     public OpenMeteoDailyData Daily { get; set; } = new();
}
