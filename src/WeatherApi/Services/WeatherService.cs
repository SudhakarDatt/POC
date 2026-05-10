using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.Models;
using WeatherApi.Models.DTOs;

namespace WeatherApi.Services;

public class WeatherService : IWeatherService
{
    private readonly WeatherDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        WeatherDbContext context,
        IEncryptionService encryptionService,
        ILogger<WeatherService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
    {
        try
        {
            var weatherData = await _context.WeatherData
                .Where(w => w.City.ToLower() == city.ToLower() && !w.IsArchived)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefaultAsync();

            if (weatherData == null)
            {
                return null;
            }

            return new WeatherResponse
            {
                City = weatherData.City,
                Country = weatherData.Country,
                Temperature = weatherData.Temperature,
                FeelsLike = weatherData.FeelsLike,
                Humidity = weatherData.Humidity,
                Pressure = weatherData.Pressure,
                Description = weatherData.Description,
                WindSpeed = weatherData.WindSpeed,
                WindDirection = weatherData.WindDirection,
                Visibility = weatherData.Visibility,
                Timestamp = weatherData.Timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current weather for city: {City}", city);
            throw;
        }
    }

    public async Task<WeatherForecastResponse> GetWeatherForecastAsync(string city, int days)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(days);

            var forecastData = await _context.WeatherData
                .Where(w => w.City.ToLower() == city.ToLower() 
                    && w.Timestamp >= startDate 
                    && w.Timestamp < endDate
                    && !w.IsArchived)
                .OrderBy(w => w.Timestamp)
                .ToListAsync();

            var groupedByDay = forecastData
                .GroupBy(w => w.Timestamp.Date)
                .Select(g => new DailyForecast
                {
                    Date = g.Key,
                    TemperatureMax = g.Max(w => w.Temperature),
                    TemperatureMin = g.Min(w => w.Temperature),
                    Description = g.First().Description,
                    Humidity = (int)g.Average(w => w.Humidity),
                    WindSpeed = g.Average(w => w.WindSpeed)
                })
                .ToList();

            var country = forecastData.FirstOrDefault()?.Country ?? "Unknown";

            return new WeatherForecastResponse
            {
                City = city,
                Country = country,
                Forecast = groupedByDay
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather forecast for city: {City}", city);
            throw;
        }
    }

    public async Task<WeatherData> CreateWeatherDataAsync(CreateWeatherDataRequest request)
    {
        try
        {
            var weatherData = new WeatherData
            {
                City = request.City,
                Country = request.Country,
                Temperature = request.Temperature,
                FeelsLike = request.FeelsLike,
                Humidity = request.Humidity,
                Pressure = request.Pressure,
                Description = request.Description,
                WindSpeed = request.WindSpeed,
                WindDirection = request.WindDirection,
                Visibility = request.Visibility,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeatherData.Add(weatherData);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Weather data created for city: {City}", request.City);

            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weather data");
            throw;
        }
    }

    public async Task<bool> DeleteWeatherDataAsync(int id)
    {
        try
        {
            var weatherData = await _context.WeatherData.FindAsync(id);
            
            if (weatherData == null)
            {
                return false;
            }

            // Soft delete by archiving
            weatherData.IsArchived = true;
            weatherData.ArchivedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Weather data archived with ID: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weather data with ID: {Id}", id);
            throw;
        }
    }

    public async Task<WeatherStatistics> GetWeatherStatisticsAsync()
    {
        try
        {
            var allData = await _context.WeatherData
                .Where(w => !w.IsArchived)
                .ToListAsync();

            if (!allData.Any())
            {
                return new WeatherStatistics();
            }

            return new WeatherStatistics
            {
                TotalRecords = allData.Count,
                TotalCities = allData.Select(w => w.City).Distinct().Count(),
                AverageTemperature = allData.Average(w => w.Temperature),
                MaxTemperature = allData.Max(w => w.Temperature),
                MinTemperature = allData.Min(w => w.Temperature),
                OldestRecord = allData.Min(w => w.Timestamp),
                NewestRecord = allData.Max(w => w.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather statistics");
            throw;
        }
    }
}