using Microsoft.EntityFrameworkCore;
using WeatherAPI.Data;
using WeatherAPI.Models;

namespace WeatherAPI.Services;

public class AuditLogger : IAuditLogger
{
    private readonly WeatherDbContext _context;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(WeatherDbContext context, ILogger<AuditLogger> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(AuditLog auditLog)
    {
        try
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Audit Log: User={UserId}, Action={Action}, Resource={Resource}, Result={Result}",
                auditLog.UserId, auditLog.Action, auditLog.Resource, auditLog.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log");
            // Don't throw - audit logging should not break the main flow
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AuditLogs.Where(a => a.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }
}