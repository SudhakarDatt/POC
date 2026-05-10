using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Models;
using WeatherAPI.Models.DTOs;
using WeatherAPI.Security;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

/// <summary>
/// Weather API Controller with RBAC, input validation, and audit logging
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
    /// Get current weather for a location
    /// </summary>
    [HttpGet("current")]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<WeatherResponse>> GetCurrentWeather([FromQuery] WeatherRequest request)
    {
        try
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check RBAC permissions
            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            if (!await _rbacService.HasPermissionAsync(userId, "weather.read"))
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "GetCurrentWeather",
                    Resource = "Weather",
                    Result = "Forbidden",
                    Timestamp = DateTime.UtcNow
                });
                return Forbid();
            }

            // Get weather data
            var weather = await _weatherService.GetCurrentWeatherAsync(request.Location);

            // Log successful access
            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "GetCurrentWeather",
                Resource = $"Weather/{request.Location}",
                Result = "Success",
                Timestamp = DateTime.UtcNow
            });

            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current weather");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get weather forecast for a location
    /// </summary>
    [HttpGet("forecast")]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<IEnumerable<WeatherResponse>>> GetForecast([FromQuery] ForecastRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            if (!await _rbacService.HasPermissionAsync(userId, "weather.read"))
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "GetForecast",
                    Resource = "Weather",
                    Result = "Forbidden",
                    Timestamp = DateTime.UtcNow
                });
                return Forbid();
            }

            var forecast = await _weatherService.GetForecastAsync(request.Location, request.Days);

            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "GetForecast",
                Resource = $"Weather/{request.Location}",
                Result = "Success",
                Timestamp = DateTime.UtcNow
            });

            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather forecast");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Create a weather alert (Admin only)
    /// </summary>
    [HttpPost("alert")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WeatherAlert>> CreateAlert([FromBody] CreateWeatherAlertRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            if (!await _rbacService.HasPermissionAsync(userId, "weather.write"))
            {
                await _auditLogger.LogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "CreateAlert",
                    Resource = "WeatherAlert",
                    Result = "Forbidden",
                    Timestamp = DateTime.UtcNow
                });
                return Forbid();
            }

            var alert = await _weatherService.CreateAlertAsync(request);

            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "CreateAlert",
                Resource = $"WeatherAlert/{alert.Id}",
                Result = "Success",
                Timestamp = DateTime.UtcNow,
                Details = $"Alert created for {request.Location}"
            });

            return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weather alert");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get weather alert by ID
    /// </summary>
    [HttpGet("alert/{id}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<WeatherAlert>> GetAlert(int id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            if (!await _rbacService.HasPermissionAsync(userId, "weather.read"))
            {
                return Forbid();
            }

            var alert = await _weatherService.GetAlertByIdAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            return Ok(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather alert");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get user consent status
    /// </summary>
    [HttpGet("consent")]
    [Authorize]
    public async Task<ActionResult<ConsentStatus>> GetConsentStatus()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            var consent = await _weatherService.GetConsentStatusAsync(userId);
            return Ok(consent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consent status");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update user consent
    /// </summary>
    [HttpPost("consent")]
    [Authorize]
    public async Task<ActionResult> UpdateConsent([FromBody] UpdateConsentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("sub")?.Value ?? "anonymous";
            await _weatherService.UpdateConsentAsync(userId, request);

            await _auditLogger.LogAsync(new AuditLog
            {
                UserId = userId,
                Action = "UpdateConsent",
                Resource = "UserConsent",
                Result = "Success",
                Timestamp = DateTime.UtcNow
            });

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consent");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}