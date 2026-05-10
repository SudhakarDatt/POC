using WeatherAPI.Security;

namespace WeatherAPI.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        
        await auditLogger.LogAsync($"{method} {path}", userId);
        await _next(context);
    }
}