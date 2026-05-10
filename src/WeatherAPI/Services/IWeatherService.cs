using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;

namespace WeatherAPI.Services;

public interface IWeatherService
{
    Task<WeatherResponse> GetCurrentWeatherAsync(string location);
    Task<IEnumerable<WeatherResponse>> GetForecastAsync(string location, int days);
    Task<WeatherAlert> CreateAlertAsync(CreateWeatherAlertRequest request);
    Task<WeatherAlert?> GetAlertByIdAsync(int id);
    Task<ConsentStatus> GetConsentStatusAsync(string userId);
    Task UpdateConsentAsync(string userId, UpdateConsentRequest request);
}