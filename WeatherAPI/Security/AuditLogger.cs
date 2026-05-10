namespace WeatherAPI.Security;

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public async Task LogAsync(string action, string userId, object? details = null)
    {
        _logger.LogInformation("Audit: User={UserId}, Action={Action}", userId, action);
        await Task.CompletedTask;
    }
}