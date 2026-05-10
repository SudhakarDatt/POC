using WeatherApi.Models;
using WeatherApi.Models.DTOs;

namespace WeatherApi.Services;

public interface IWeatherService
{
    Task<WeatherResponse?> GetCurrentWeatherAsync(string city);
    Task<WeatherForecastResponse> GetWeatherForecastAsync(string city, int days);
    Task<WeatherData> CreateWeatherDataAsync(CreateWeatherDataRequest request);
    Task<bool> DeleteWeatherDataAsync(int id);
    Task<WeatherStatistics> GetWeatherStatisticsAsync();
}