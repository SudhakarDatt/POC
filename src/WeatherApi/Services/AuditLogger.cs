using Microsoft.EntityFrameworkCore;
using WeatherApi.Data;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class AuditLogger : IAuditLogger
{
    private readonly WeatherDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(
        WeatherDbContext context,
        IConfiguration configuration,
        ILogger<AuditLogger> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        try
        {
            // Set retention expiry date based on configuration
            var retentionDays = _configuration.GetValue<int>("DataRetention:AuditLogRetentionDays", 2555); // 7 years default
            auditLog.RetentionExpiryDate = DateTime.UtcNow.AddDays(retentionDays);

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {Action} by {UserId}", auditLog.Action, auditLog.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            // Don't throw - audit logging should not break the application
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(1000) // Limit results
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            throw;
        }
    }
}