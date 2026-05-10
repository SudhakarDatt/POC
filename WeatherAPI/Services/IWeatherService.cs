using WeatherAPI.Models;

namespace WeatherAPI.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherAsync(string city);
    Task<WeatherResponse> CreateWeatherDataAsync(WeatherRequest request);
}