using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;

namespace WeatherApi.Services;

/// <summary>
/// Data retention service for compliance (GDPR, HIPAA, SOC2)
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

    public async Task ArchiveExpiredDataAsync()
    {
        try
        {
            var retentionDays = _configuration.GetValue<int>("DataRetention:WeatherDataRetentionDays", 365);
            var expiryDate = DateTime.UtcNow.AddDays(-retentionDays);

            var expiredData = await _context.WeatherData
                .Where(w => w.CreatedAt < expiryDate && !w.IsArchived)
                .ToListAsync();

            foreach (var data in expiredData)
            {
                data.IsArchived = true;
                data.ArchivedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived {Count} expired weather records", expiredData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving expired data");
            throw;
        }
    }

    public async Task DeleteExpiredAuditLogsAsync()
    {
        try
        {
            var retentionDays = _configuration.GetValue<int>("DataRetention:AuditLogRetentionDays", 2555); // 7 years
            var expiryDate = DateTime.UtcNow.AddDays(-retentionDays);

            var expiredLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < expiryDate && a.IsRetained)
                .ToListAsync();

            foreach (var log in expiredLogs)
            {
                log.IsRetained = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} audit logs as expired", expiredLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired audit logs");
            throw;
        }
    }

    public Task<int> GetRetentionDaysAsync(string dataType)
    {
        var retentionDays = dataType.ToLower() switch
        {
            "auditlog" => _configuration.GetValue<int>("DataRetention:AuditLogRetentionDays", 2555),
            "weatherdata" => _configuration.GetValue<int>("DataRetention:WeatherDataRetentionDays", 365),
            "userdata" => _configuration.GetValue<int>("DataRetention:UserDataRetentionDays", 1825),
            _ => 365
        };

        return Task.FromResult(retentionDays);
    }
}