using WeatherAPI.Models;

namespace WeatherAPI.Services;

public interface IAuditLogger
{
    Task LogAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}