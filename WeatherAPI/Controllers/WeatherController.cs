using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{city}")]
    public async Task<ActionResult<WeatherResponse>> GetWeather(string city)
    {
        var weather = await _weatherService.GetWeatherAsync(city);
        return weather == null ? NotFound() : Ok(weather);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WeatherResponse>> CreateWeather([FromBody] WeatherRequest request)
    {
        var weather = await _weatherService.CreateWeatherDataAsync(request);
        return CreatedAtAction(nameof(GetWeather), new { city = weather.City }, weather);
    }
}