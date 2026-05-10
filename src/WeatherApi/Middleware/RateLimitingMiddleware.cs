using System.Collections.Concurrent;
using System.Net;

namespace WeatherApi.Middleware;

/// <summary>
/// Rate limiting middleware to prevent abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var requestsPerMinute = _configuration.GetValue<int>("RateLimiting:RequestsPerMinute", 60);

        var clientInfo = _clients.GetOrAdd(clientId, _ => new ClientRequestInfo());

        lock (clientInfo)
        {
            var now = DateTime.UtcNow;
            
            // Remove old requests (older than 1 minute)
            clientInfo.RequestTimestamps.RemoveAll(t => (now - t).TotalMinutes > 1);

            if (clientInfo.RequestTimestamps.Count >= requestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", "60");
                return;
            }

            clientInfo.RequestTimestamps.Add(now);
        }

        await _next(context);
    }

    private class ClientRequestInfo
    {
        public List<DateTime> RequestTimestamps { get; set; } = new();
    }
}