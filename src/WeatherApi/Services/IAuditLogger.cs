using WeatherApi.Models;

namespace WeatherApi.Services;

public interface IAuditLogger
{
    Task LogAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null);
}