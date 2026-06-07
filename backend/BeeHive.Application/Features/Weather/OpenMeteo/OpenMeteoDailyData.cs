using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Weather.OpenMeteo;

internal sealed class OpenMeteoDailyData
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double?>? Temperature2mMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double?>? Temperature2mMin { get; set; }

    [JsonPropertyName("weathercode")]
    public List<double?>? Weathercode { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double?>? PrecipitationSum { get; set; }

    [JsonPropertyName("windspeed_10m_max")]
    public List<double?>? Windspeed10mMax { get; set; }

    [JsonPropertyName("precipitation_probability_max")]
    public List<double?>? PrecipitationProbabilityMax { get; set; }
}
