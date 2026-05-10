using Microsoft.EntityFrameworkCore;
using WeatherAPI.Data;

namespace WeatherAPI.Services;

/// <summary>
/// Data retention service for compliance with GDPR, HIPAA, SOC2
/// </summary>
public class DataRetentionService : IDataRetentionService
{
    private readonly WeatherDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(
        WeatherDbContext context,
        IConfiguration configuration,
        ILogger<DataRetentionService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task CleanupExpiredDataAsync()
    {
        try
        {
            var auditRetentionDays = await GetRetentionDaysAsync("AuditLog");
            var weatherRetentionDays = await GetRetentionDaysAsync("WeatherData");

            var auditCutoffDate = DateTime.UtcNow.AddDays(-auditRetentionDays);
            var weatherCutoffDate = DateTime.UtcNow.AddDays(-weatherRetentionDays);

            // Delete expired audit logs
            var expiredAuditLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < auditCutoffDate)
                .ToListAsync();

            if (expiredAuditLogs.Any())
            {
                _context.AuditLogs.RemoveRange(expiredAuditLogs);
                _logger.LogInformation("Deleted {Count} expired audit logs", expiredAuditLogs.Count);
            }

            // Delete expired weather data
            var expiredWeatherData = await _context.WeatherData
                .Where(w => w.CreatedAt < weatherCutoffDate)
                .ToListAsync();

            if (expiredWeatherData.Any())
            {
                _context.WeatherData.RemoveRange(expiredWeatherData);
                _logger.LogInformation("Deleted {Count} expired weather records", expiredWeatherData.Count);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data retention cleanup");
            throw;
        }
    }

    public async Task<int> GetRetentionDaysAsync(string dataType)
    {
        await Task.CompletedTask;
        
        return dataType switch
        {
            "AuditLog" => _configuration.GetValue<int>("DataRetention:AuditLogRetentionDays", 2555), // 7 years for compliance
            "WeatherData" => _configuration.GetValue<int>("DataRetention:WeatherDataRetentionDays", 365),
            _ => 90 // Default 90 days
        };
    }
}