using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApi.Models;
using WeatherApi.Models.DTOs;
using WeatherApi.Services;
using WeatherApi.Security;
using System.Security.Claims;

namespace WeatherApi.Controllers;

/// <summary>
/// Weather API Controller with RBAC, audit logging, and input validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IAuditLogger _auditLogger;
    private readonly IRBACService _rbacService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IWeatherService weatherService,
        IAuditLogger auditLogger,
        IRBACService rbacService,
        ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _auditLogger = auditLogger;
        _rbacService = rbacService;
        _logger = logger;
    }

    /// <summary>
    /// Get current weather by city name
    /// </summary>
    /// <param name="city">City name</param>
    /// <returns>Weather data</returns>
    [HttpGet("current/{city}")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherResponse>> GetCurrentWeather(string city)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            
            // Audit log the request
            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "GetCurrentWeather",
                Resource = $"Weather/Current/{city}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
            });

            var weather = await _weatherService.GetCurrentWeatherAsync(city);
            
            if (weather == null)
            {
                _logger.LogWarning("Weather data not found for city: {City}", city);
                return NotFound(new { message = $"Weather data not found for city: {city}" });
            }

            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather for city: {City}", city);
            throw;
        }
    }

    /// <summary>
    /// Get weather forecast for multiple days
    /// </summary>
    /// <param name="request">Forecast request parameters</param>
    /// <returns>Weather forecast data</returns>
    [HttpPost("forecast")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WeatherForecastResponse>> GetWeatherForecast([FromBody] WeatherForecastRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            
            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "GetWeatherForecast",
                Resource = $"Weather/Forecast/{request.City}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"Days: {request.Days}"
            });

            var forecast = await _weatherService.GetWeatherForecastAsync(request.City, request.Days);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast for city: {City}", request.City);
            throw;
        }
    }

    /// <summary>
    /// Create or update weather data (Admin only)
    /// </summary>
    /// <param name="request">Weather data to create/update</param>
    /// <returns>Created weather data</returns>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WeatherData), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WeatherData>> CreateWeatherData([FromBody] CreateWeatherDataRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            
            // Check RBAC permissions
            if (!await _rbacService.HasPermissionAsync(userId, "weather.write"))
            {
                _logger.LogWarning("User {UserId} attempted unauthorized weather data creation", userId);
                return Forbid();
            }

            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "CreateWeatherData",
                Resource = "Weather",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"City: {request.City}, Temperature: {request.Temperature}"
            });

            var weatherData = await _weatherService.CreateWeatherDataAsync(request);
            return CreatedAtAction(nameof(GetCurrentWeather), new { city = weatherData.City }, weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weather data");
            throw;
        }
    }

    /// <summary>
    /// Delete weather data (Admin only)
    /// </summary>
    /// <param name="id">Weather data ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWeatherData(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            
            if (!await _rbacService.HasPermissionAsync(userId, "weather.delete"))
            {
                _logger.LogWarning("User {UserId} attempted unauthorized weather data deletion", userId);
                return Forbid();
            }

            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "DeleteWeatherData",
                Resource = $"Weather/{id}",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            var result = await _weatherService.DeleteWeatherDataAsync(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weather data with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get weather statistics (Admin only)
    /// </summary>
    /// <returns>Weather statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WeatherStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<WeatherStatistics>> GetWeatherStatistics()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            
            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "GetWeatherStatistics",
                Resource = "Weather/Statistics",
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            var statistics = await _weatherService.GetWeatherStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather statistics");
            throw;
        }
    }
}