namespace WeatherAPI.Security;

public interface IAuditLogger
{
    Task LogAsync(string action, string userId, object? details = null);
    Task<IEnumerable<Models.AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? userId = null);
}