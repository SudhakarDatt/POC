using System.Text.Json;
using WeatherAPI.Models;

namespace WeatherAPI.Security;

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IEncryptionService _encryptionService;
    private readonly List<AuditLog> _auditLogs = new();
    private readonly IConfiguration _configuration;

    public AuditLogger(
        ILogger<AuditLogger> logger,
        IEncryptionService encryptionService,
        IConfiguration configuration)
    {
        _logger = logger;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public async Task LogAsync(string action, string userId, object? details = null)
    {
        try
        {
            var retentionDays = _configuration.GetValue<int>("DataRetention:RetentionDays", 90);
            
            var auditLog = new AuditLog
            {
                Id = _auditLogs.Count + 1,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Action = action,
                Resource = "WeatherAPI",
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                IpAddress = "127.0.0.1",
                UserAgent = "WeatherAPI Client",
                Status = "Success",
                RetentionExpiry = DateTime.UtcNow.AddDays(retentionDays)
            };

            if (!string.IsNullOrEmpty(auditLog.Details))
            {
                auditLog.Details = _encryptionService.Encrypt(auditLog.Details);
            }

            _auditLogs.Add(auditLog);

            _logger.LogInformation(
                "Audit Log: User={UserId}, Action={Action}, Timestamp={Timestamp}",
                userId, action, auditLog.Timestamp);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for action: {Action}", action);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? userId = null)
    {
        try
        {
            var query = _auditLogs.AsEnumerable();

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(log => log.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            }

            var logs = query.ToList();

            foreach (var log in logs.Where(l => !string.IsNullOrEmpty(l.Details)))
            {
                try
                {
                    log.Details = _encryptionService.Decrypt(log.Details!);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decrypt audit log details for log ID: {LogId}", log.Id);
                }
            }

            return await Task.FromResult(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            throw;
        }
    }
}