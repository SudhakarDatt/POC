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
    public async Task<ActionResult<WeatherResponse>> GetWeather(
        [Required][StringLength(100, MinimumLength = 2)] string city)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.Identity?.Name ?? "Anonymous";
        await _auditLogger.LogAsync("GetWeather", userId, new { City = city });

        var weather = await _weatherService.GetWeatherAsync(city);
        if (weather == null)
            return NotFound();

        return Ok(weather);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<WeatherResponse>> CreateWeatherData(
        [FromBody][Required] WeatherRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.Identity?.Name ?? "Anonymous";
        await _auditLogger.LogAsync("CreateWeatherData", userId, request);

        var weather = await _weatherService.CreateWeatherDataAsync(request);
        return CreatedAtAction(nameof(GetWeather), new { city = weather.City }, weather);
    }
}
