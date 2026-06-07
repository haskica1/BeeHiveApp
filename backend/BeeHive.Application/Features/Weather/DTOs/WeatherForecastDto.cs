namespace BeeHive.Application.Features.Weather.DTOs;

public class WeatherForecastDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public List<DailyWeatherDto> Daily { get; set; } = [];
}
