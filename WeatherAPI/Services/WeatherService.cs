using WeatherAPI.Models;

namespace WeatherAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly List<WeatherResponse> _data = new();
    private int _nextId = 1;

    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        return await Task.FromResult(_data.FirstOrDefault(w => 
            w.City.Equals(city, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task<WeatherResponse> CreateWeatherDataAsync(WeatherRequest request)
    {
        var weather = new WeatherResponse
        {
            Id = _nextId++,
            City = request.City,
            Country = request.Country,
            Temperature = request.Temperature,
            Humidity = request.Humidity,
            WindSpeed = request.WindSpeed,
            Condition = request.Condition,
            Pressure = request.Pressure,
            Visibility = request.Visibility,
            Timestamp = DateTime.UtcNow
        };
        _data.Add(weather);
        return await Task.FromResult(weather);
    }
}