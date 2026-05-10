namespace WeatherAPI.Security;

public interface IAuditLogger
{
    Task LogAsync(string action, string userId, object? details = null);
}