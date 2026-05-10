using Microsoft.EntityFrameworkCore;
using WeatherAPI.Data;
using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;

namespace WeatherAPI.Services;

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

    public async Task<WeatherResponse> GetCurrentWeatherAsync(string location)
    {
        // In production, this would call an external weather API
        // For demo purposes, we'll return mock data
        var weatherData = await _context.WeatherData
            .Where(w => w.Location.ToLower() == location.ToLower())
            .OrderByDescending(w => w.Timestamp)
            .FirstOrDefaultAsync();

        if (weatherData == null)
        {
            // Generate mock data
            weatherData = new WeatherData
            {
                Location = location,
                Temperature = Random.Shared.Next(-10, 40),
                Humidity = Random.Shared.Next(30, 90),
                WindSpeed = Random.Shared.Next(0, 50),
                Condition = GetRandomCondition(),
                Timestamp = DateTime.UtcNow
            };

            _context.WeatherData.Add(weatherData);
            await _context.SaveChangesAsync();
        }

        return new WeatherResponse
        {
            Location = weatherData.Location,
            Temperature = weatherData.Temperature,
            Humidity = weatherData.Humidity,
            WindSpeed = weatherData.WindSpeed,
            Condition = weatherData.Condition,
            Timestamp = weatherData.Timestamp
        };
    }

    public async Task<IEnumerable<WeatherResponse>> GetForecastAsync(string location, int days)
    {
        var forecast = new List<WeatherResponse>();
        var baseDate = DateTime.UtcNow;

        for (int i = 0; i < days; i++)
        {
            forecast.Add(new WeatherResponse
            {
                Location = location,
                Temperature = Random.Shared.Next(-10, 40),
                Humidity = Random.Shared.Next(30, 90),
                WindSpeed = Random.Shared.Next(0, 50),
                Condition = GetRandomCondition(),
                Timestamp = baseDate.AddDays(i)
            });
        }

        return await Task.FromResult(forecast);
    }

    public async Task<WeatherAlert> CreateAlertAsync(CreateWeatherAlertRequest request)
    {
        var alert = new WeatherAlert
        {
            Location = request.Location,
            AlertType = request.AlertType,
            Description = request.Description,
            Severity = request.Severity,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.WeatherAlerts.Add(alert);
        await _context.SaveChangesAsync();

        return alert;
    }

    public async Task<WeatherAlert?> GetAlertByIdAsync(int id)
    {
        return await _context.WeatherAlerts.FindAsync(id);
    }

    public async Task<ConsentStatus> GetConsentStatusAsync(string userId)
    {
        var consent = await _context.UserConsents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ConsentDate)
            .FirstOrDefaultAsync();

        if (consent == null)
        {
            return new ConsentStatus
            {
                DataCollectionConsent = false,
                DataProcessingConsent = false,
                MarketingConsent = false,
                ConsentDate = DateTime.MinValue,
                ConsentVersion = "1.0"
            };
        }

        return new ConsentStatus
        {
            DataCollectionConsent = consent.DataCollectionConsent,
            DataProcessingConsent = consent.DataProcessingConsent,
            MarketingConsent = consent.MarketingConsent,
            ConsentDate = consent.ConsentDate,
            ConsentVersion = consent.ConsentVersion
        };
    }

    public async Task UpdateConsentAsync(string userId, UpdateConsentRequest request)
    {
        var consent = new UserConsent
        {
            UserId = userId,
            DataCollectionConsent = request.DataCollectionConsent,
            DataProcessingConsent = request.DataProcessingConsent,
            MarketingConsent = request.MarketingConsent,
            ConsentDate = DateTime.UtcNow,
            ConsentVersion = "1.0"
        };

        _context.UserConsents.Add(consent);
        await _context.SaveChangesAsync();
    }

    private static string GetRandomCondition()
    {
        var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Stormy", "Snowy", "Foggy", "Windy" };
        return conditions[Random.Shared.Next(conditions.Length)];
    }
}