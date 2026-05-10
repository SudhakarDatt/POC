using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IWeatherService weatherService,
        IAuditLogger auditLogger,
        ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    [HttpGet("{city}")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherResponse>> GetWeather(
        [Required][StringLength(100, MinimumLength = 2)] string city)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for city: {City}", city);
                return BadRequest(ModelState);
            }

            var userId = User.Identity?.Name ?? "Anonymous";
            await _auditLogger.LogAsync("GetWeather", userId, new { City = city });

            var weather = await _weatherService.GetWeatherAsync(city);
            
            if (weather == null)
            {
                _logger.LogWarning("Weather data not found for city: {City}", city);
                return NotFound(new { Message = $"Weather data not found for {city}" });
            }

            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather for city: {City}", city);
            throw;
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WeatherResponse>> CreateWeatherData(
        [FromBody][Required] WeatherRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid weather request model");
                return BadRequest(ModelState);
            }

            var userId = User.Identity?.Name ?? "Anonymous";
            await _auditLogger.LogAsync("CreateWeatherData", userId, request);

            var weather = await _weatherService.CreateWeatherDataAsync(request);
            
            return CreatedAtAction(
                nameof(GetWeather),
                new { city = weather.City },
                weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weather data");
            throw;
        }
    }

    [HttpPut("{city}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeatherResponse>> UpdateWeatherData(
        [Required] string city,
        [FromBody][Required] WeatherRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity?.Name ?? "Anonymous";
            await _auditLogger.LogAsync("UpdateWeatherData", userId, new { City = city, Request = request });

            var weather = await _weatherService.UpdateWeatherDataAsync(city, request);
            
            if (weather == null)
            {
                return NotFound(new { Message = $"Weather data not found for {city}" });
            }

            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weather data for city: {City}", city);
            throw;
        }
    }

    [HttpDelete("{city}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWeatherData([Required] string city)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Anonymous";
            await _auditLogger.LogAsync("DeleteWeatherData", userId, new { City = city });

            var result = await _weatherService.DeleteWeatherDataAsync(city);
            
            if (!result)
            {
                return NotFound(new { Message = $"Weather data not found for {city}" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weather data for city: {City}", city);
            throw;
        }
    }

    [HttpGet("forecast/{city}")]
    [Authorize(Policy = "UserOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<WeatherResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WeatherResponse>>> GetForecast(
        [Required] string city,
        [Range(1, 14)] int days = 7)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Anonymous";
            await _auditLogger.LogAsync("GetForecast", userId, new { City = city, Days = days });

            var forecast = await _weatherService.GetForecastAsync(city, days);
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast for city: {City}", city);
            throw;
        }
    }
}