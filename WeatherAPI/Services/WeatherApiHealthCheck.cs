using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WeatherAPI.Services;

public class WeatherApiHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Weather API is healthy"));
    }
}