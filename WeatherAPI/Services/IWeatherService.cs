using WeatherAPI.Models;

namespace WeatherAPI.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherAsync(string city);
    Task<WeatherResponse> CreateWeatherDataAsync(WeatherRequest request);
    Task<WeatherResponse?> UpdateWeatherDataAsync(string city, WeatherRequest request);
    Task<bool> DeleteWeatherDataAsync(string city);
    Task<IEnumerable<WeatherResponse>> GetForecastAsync(string city, int days);
}