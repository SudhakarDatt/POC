using Microsoft.AspNetCore.Mvc;
using WeatherAPI.API.Models;
using WeatherAPI.API.Services;
using System.Diagnostics;

namespace WeatherAPI.API.Controllers
{
    /// <summary>
    /// Weather API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get weather information by city name or coordinates
        /// </summary>
        /// <param name="city">City name (optional if lat/lon provided)</param>
        /// <param name="lat">Latitude (optional if city provided)</param>
        /// <param name="lon">Longitude (optional if city provided)</param>
        /// <param name="unit">Unit system: metric or imperial (default: metric)</param>
        /// <returns>Weather information</returns>
        /// <response code="200">Returns weather data</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="404">Location not found</response>
        /// <response code="408">Request timeout</response>
        /// <response code="429">Rate limit exceeded</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status408RequestTimeout)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WeatherResponse>> GetWeather(
            [FromQuery] string? city,
            [FromQuery] double? lat,
            [FromQuery] double? lon,
            [FromQuery] string unit = "metric")
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var request = new WeatherRequest
                {
                    City = city,
                    Latitude = lat,
                    Longitude = lon,
                    Unit = unit
                };

                if (!request.IsValid())
                {
                    _logger.LogWarning("Invalid weather request: missing city or coordinates");
                    return BadRequest(new ErrorResponse
                    {
                        Error = "BadRequest",
                        Message = "Either city name or coordinates (latitude and longitude) must be provided",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                _logger.LogInformation("Processing weather request for {Location}", 
                    !string.IsNullOrWhiteSpace(city) ? $"city: {city}" : $"coordinates: {lat}, {lon}");

                var weather = await _weatherService.GetWeatherAsync(request, HttpContext.RequestAborted);

                stopwatch.Stop();
                _logger.LogInformation("Weather request completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return Ok(weather);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
                return BadRequest(new ErrorResponse
                {
                    Error = "BadRequest",
                    Message = ex.Message,
                    StatusCode = 400,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Location not found: {Message}", ex.Message);
                return NotFound(new ErrorResponse
                {
                    Error = "NotFound",
                    Message = ex.Message,
                    StatusCode = 404,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Request timeout: {Message}", ex.Message);
                return StatusCode(408, new ErrorResponse
                {
                    Error = "RequestTimeout",
                    Message = ex.Message,
                    StatusCode = 408,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing weather request");
                return StatusCode(500, new ErrorResponse
                {
                    Error = "InternalServerError",
                    Message = "An unexpected error occurred",
                    StatusCode = 500,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}