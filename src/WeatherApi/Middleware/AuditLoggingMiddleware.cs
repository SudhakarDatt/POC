using WeatherApi.Models;
using WeatherApi.Services;
using System.Security.Claims;

namespace WeatherApi.Middleware;

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
        Exception? exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            // Skip logging for health checks and swagger
            if (!context.Request.Path.StartsWithSegments("/health") &&
                !context.Request.Path.StartsWithSegments("/swagger"))
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
                var action = $"{context.Request.Method} {context.Request.Path}";

                await auditLogger.LogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Resource = context.Request.Path,
                    Timestamp = startTime,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    IsSuccessful = exception == null && context.Response.StatusCode < 400,
                    ErrorMessage = exception?.Message,
                    Details = $"Status: {context.Response.StatusCode}"
                });
            }
        }
    }
}