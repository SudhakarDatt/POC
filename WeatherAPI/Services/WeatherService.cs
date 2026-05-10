using WeatherAPI.Models;
using WeatherAPI.Security;

namespace WeatherAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<WeatherService> _logger;
    private readonly List<WeatherData> _weatherDataStore = new();
    private int _nextId = 1;

    public WeatherService(
        IEncryptionService encryptionService,
        ILogger<WeatherService> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        _weatherDataStore.AddRange(new[]
        {
            new WeatherData
            {
                Id = _nextId++,
                City = "London",
                Country = "UK",
                Temperature = 15.5,
                Humidity = 72,
                WindSpeed = 12.5,
                Condition = "Cloudy",
                Pressure = 1013.25,
                Visibility = 10.0,
                CreatedBy = "System"
            },
            new WeatherData
            {
                Id = _nextId++,
                City = "New York",
                Country = "USA",
                Temperature = 22.0,
                Humidity = 65,
                WindSpeed = 8.0,
                Condition = "Sunny",
                Pressure = 1015.0,
                Visibility = 15.0,
                CreatedBy = "System"
            },
            new WeatherData
            {
                Id = _nextId++,
                City = "Tokyo",
                Country = "Japan",
                Temperature = 18.0,
                Humidity = 80,
                WindSpeed = 15.0,
                Condition = "Rainy",
                Pressure = 1010.0,
                Visibility = 8.0,
                CreatedBy = "System"
            }
        });
    }

    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        try
        {
            var weatherData = _weatherDataStore
                .FirstOrDefault(w => w.City.Equals(city, StringComparison.OrdinalIgnoreCase) && !w.IsDeleted);

            if (weatherData == null)
            {
                _logger.LogInformation("Weather data not found for city: {City}", city);
                return null;
            }

            return await Task.FromResult(MapToResponse(weatherData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetWeatherAsync for city: {City}", city);
            throw;
        }
    }

    public async Task<WeatherResponse> CreateWeatherDataAsync(WeatherRequest request)
    {
        try
        {
            var existingData = _weatherDataStore
                .FirstOrDefault(w => w.City.Equals(request.City, StringComparison.OrdinalIgnoreCase) && !w.IsDeleted);

            if (existingData != null)
            {
                throw new InvalidOperationException($"Weather data already exists for {request.City}");
            }

            var weatherData = new WeatherData
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
                CreatedBy = "Admin"
            };

            _weatherDataStore.Add(weatherData);
            _logger.LogInformation("Created weather data for city: {City}", request.City);

            return await Task.FromResult(MapToResponse(weatherData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateWeatherDataAsync");
            throw;
        }
    }

    public async Task<WeatherResponse?> UpdateWeatherDataAsync(string city, WeatherRequest request)
    {
        try
        {
            var weatherData = _weatherDataStore
                .FirstOrDefault(w => w.City.Equals(city, StringComparison.OrdinalIgnoreCase) && !w.IsDeleted);

            if (weatherData == null)
            {
                _logger.LogWarning("Weather data not found for update: {City}", city);
                return null;
            }

            weatherData.Country = request.Country;
            weatherData.Temperature = request.Temperature;
            weatherData.Humidity = request.Humidity;
            weatherData.WindSpeed = request.WindSpeed;
            weatherData.Condition = request.Condition;
            weatherData.Pressure = request.Pressure;
            weatherData.Visibility = request.Visibility;
            weatherData.UpdatedAt = DateTime.UtcNow;
            weatherData.UpdatedBy = "Admin";
            weatherData.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Updated weather data for city: {City}", city);

            return await Task.FromResult(MapToResponse(weatherData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateWeatherDataAsync for city: {City}", city);
            throw;
        }
    }

    public async Task<bool> DeleteWeatherDataAsync(string city)
    {
        try
        {
            var weatherData = _weatherDataStore
                .FirstOrDefault(w => w.City.Equals(city, StringComparison.OrdinalIgnoreCase) && !w.IsDeleted);

            if (weatherData == null)
            {
                _logger.LogWarning("Weather data not found for deletion: {City}", city);
                return false;
            }

            weatherData.IsDeleted = true;
            weatherData.DeletedAt = DateTime.UtcNow;

            _logger.LogInformation("Deleted weather data for city: {City}", city);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteWeatherDataAsync for city: {City}", city);
            throw;
        }
    }

    public async Task<IEnumerable<WeatherResponse>> GetForecastAsync(string city, int days)
    {
        try
        {
            var currentWeather = await GetWeatherAsync(city);
            
            if (currentWeather == null)
            {
                return Enumerable.Empty<WeatherResponse>();
            }

            var forecast = new List<WeatherResponse>();
            var random = new Random();

            for (int i = 0; i < days; i++)
            {
                forecast.Add(new WeatherResponse
                {
                    Id = currentWeather.Id,
                    City = currentWeather.City,
                    Country = currentWeather.Country,
                    Temperature = currentWeather.Temperature + random.Next(-5, 6),
                    Humidity = Math.Clamp(currentWeather.Humidity + random.Next(-10, 11), 0, 100),
                    WindSpeed = Math.Max(0, currentWeather.WindSpeed + random.Next(-5, 6)),
                    Condition = GetRandomCondition(random),
                    Pressure = currentWeather.Pressure + random.Next(-20, 21),
                    Visibility = Math.Clamp(currentWeather.Visibility + random.Next(-3, 4), 0, 100),
                    Timestamp = DateTime.UtcNow.AddDays(i),
                    CreatedAt = DateTime.UtcNow
                });
            }

            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetForecastAsync for city: {City}", city);
            throw;
        }
    }

    private static string GetRandomCondition(Random random)
    {
        var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Windy", "Foggy" };
        return conditions[random.Next(conditions.Length)];
    }

    private static WeatherResponse MapToResponse(WeatherData data)
    {
        return new WeatherResponse
        {
            Id = data.Id,
            City = data.City,
            Country = data.Country,
            Temperature = data.Temperature,
            Humidity = data.Humidity,
            WindSpeed = data.WindSpeed,
            Condition = data.Condition,
            Pressure = data.Pressure,
            Visibility = data.Visibility,
            Timestamp = data.Timestamp,
            CreatedAt = data.CreatedAt
        };
    }
}