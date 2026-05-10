using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Middleware;

/// <summary>
/// Middleware for automatic audit logging of all requests
/// </summary>
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            await _next(context);
        }
        finally
        {
            // Log request after completion
            var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
            var action = $"{context.Request.Method} {context.Request.Path}";
            var statusCode = context.Response.StatusCode;
            var result = statusCode >= 200 && statusCode < 300 ? "Success" : "Failed";

            await auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = action,
                Resource = context.Request.Path,
                Result = result,
                Timestamp = startTime,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Details = $"Status: {statusCode}, Duration: {(DateTime.UtcNow - startTime).TotalMilliseconds}ms"
            });
        }
    }
}