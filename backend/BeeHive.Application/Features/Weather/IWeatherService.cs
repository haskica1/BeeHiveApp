namespace BeeHive.Application.Features.Weather;

public interface IWeatherService
{
    Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude);
}
