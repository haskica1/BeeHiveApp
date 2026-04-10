using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BeeHive.Application.Features.Weather;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public class WeatherForecastDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public List<DailyWeatherDto> Daily { get; set; } = [];
}

public class DailyWeatherDto
{
    public string Date { get; set; } = string.Empty;
    public double? MaxTemp { get; set; }
    public double? MinTemp { get; set; }
    public int? WeatherCode { get; set; }
    public double? PrecipitationSum { get; set; }
    public double? MaxWindSpeed { get; set; }
    public double? PrecipitationProbability { get; set; }
}

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IWeatherService
{
    Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;

    public WeatherService(HttpClient http) => _http = http;

    public async Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude)
    {
        // Open-Meteo is free and requires no API key.
        var url =
            $"https://api.open-meteo.com/v1/forecast" +
            $"?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&daily=temperature_2m_max,temperature_2m_min,weathercode," +
            $"precipitation_sum,windspeed_10m_max,precipitation_probability_max" +
            $"&timezone=auto&forecast_days=7";

        var raw = await _http.GetFromJsonAsync<OpenMeteoResponse>(url)
            ?? throw new HttpRequestException("Empty response from weather API.");

        var forecast = new WeatherForecastDto
        {
            Latitude  = raw.Latitude,
            Longitude = raw.Longitude,
            Timezone  = raw.Timezone ?? "UTC",
        };

        var d = raw.Daily;
        int count = d.Time?.Count ?? 0;
        for (int i = 0; i < count; i++)
        {
            forecast.Daily.Add(new DailyWeatherDto
            {
                Date                    = d.Time![i],
                MaxTemp                 = SafeGet(d.Temperature2mMax, i),
                MinTemp                 = SafeGet(d.Temperature2mMin, i),
                WeatherCode             = (int?)SafeGet(d.Weathercode, i),
                PrecipitationSum        = SafeGet(d.PrecipitationSum, i),
                MaxWindSpeed            = SafeGet(d.Windspeed10mMax, i),
                PrecipitationProbability = SafeGet(d.PrecipitationProbabilityMax, i),
            });
        }

        return forecast;
    }

    private static double? SafeGet(List<double?>? list, int i) =>
        list is not null && i < list.Count ? list[i] : null;
}

// ── Open-Meteo response shapes (internal) ────────────────────────────────────

internal sealed class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]  public double  Latitude  { get; set; }
    [JsonPropertyName("longitude")] public double  Longitude { get; set; }
    [JsonPropertyName("timezone")]  public string? Timezone  { get; set; }
    [JsonPropertyName("daily")]     public OpenMeteoDailyData Daily { get; set; } = new();
}

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
