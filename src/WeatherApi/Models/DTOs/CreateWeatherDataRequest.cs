using System.ComponentModel.DataAnnotations;

namespace WeatherApi.Models.DTOs;

public class CreateWeatherDataRequest
{
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City name must be between 2 and 100 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Country name must be between 2 and 50 characters")]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Range(-100, 60, ErrorMessage = "Temperature must be between -100 and 60 degrees Celsius")]
    public double Temperature { get; set; }

    [Required]
    [Range(-100, 60, ErrorMessage = "Feels like temperature must be between -100 and 60 degrees Celsius")]
    public double FeelsLike { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100")]
    public int Humidity { get; set; }

    [Required]
    [Range(800, 1100, ErrorMessage = "Pressure must be between 800 and 1100 hPa")]
    public double Pressure { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double WindSpeed { get; set; }

    [Range(0, 360, ErrorMessage = "Wind direction must be between 0 and 360 degrees")]
    public int? WindDirection { get; set; }

    [Range(0, 50000, ErrorMessage = "Visibility must be between 0 and 50000 meters")]
    public double? Visibility { get; set; }
}