using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WeatherAPI.Services;

public class WeatherApiHealthCheck : IHealthCheck
{
    private readonly ILogger<WeatherApiHealthCheck> _logger;

    public WeatherApiHealthCheck(ILogger<WeatherApiHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy(
                        "Weather API is healthy",
                        new Dictionary<string, object>
                        {
                            { "timestamp", DateTime.UtcNow },
                            { "status", "operational" },
                            { "version", "1.0.0" }
                        }));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Weather API is unhealthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Health check failed",
                    ex));
        }
    }
}