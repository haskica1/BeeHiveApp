using System.Net.Http.Json;
using BeeHive.Application.Features.Weather.DTOs;
using BeeHive.Application.Features.Weather.OpenMeteo;

namespace BeeHive.Application.Features.Weather;

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
