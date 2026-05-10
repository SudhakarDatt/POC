using WeatherAPI.Security;

namespace WeatherAPI.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var requestMethod = context.Request.Method;
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogInformation(
            "Request: {Method} {Path} from {IpAddress} by {UserId}",
            requestMethod, requestPath, ipAddress, userId);

        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation(
                "Response: {StatusCode} for {Method} {Path} - Duration: {Duration}ms",
                context.Response.StatusCode, requestMethod, requestPath, duration);

            if (ShouldAuditRequest(requestPath, requestMethod))
            {
                await auditLogger.LogAsync(
                    $"{requestMethod} {requestPath}",
                    userId,
                    new
                    {
                        StatusCode = context.Response.StatusCode,
                        Duration = duration,
                        IpAddress = ipAddress
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {Method} {Path}", requestMethod, requestPath);
            
            await auditLogger.LogAsync(
                $"{requestMethod} {requestPath}",
                userId,
                new
                {
                    Error = ex.Message,
                    IpAddress = ipAddress
                });

            throw;
        }
    }

    private static bool ShouldAuditRequest(string path, string method)
    {
        if (path.Contains("/health", StringComparison.OrdinalIgnoreCase))
            return false;

        if (path.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
            return false;

        return method != "GET" || path.Contains("/api/", StringComparison.OrdinalIgnoreCase);
    }
}