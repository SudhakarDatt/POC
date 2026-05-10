using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class WeatherRequest
{
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z\s-]+$", ErrorMessage = "City name can only contain letters, spaces, and hyphens")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country must be between 2 and 100 characters")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Temperature is required")]
    [Range(-100, 100, ErrorMessage = "Temperature must be between -100 and 100 degrees")]
    public double Temperature { get; set; }

    [Required(ErrorMessage = "Humidity is required")]
    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100 percent")]
    public int Humidity { get; set; }

    [Required(ErrorMessage = "Wind speed is required")]
    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double WindSpeed { get; set; }

    [Required(ErrorMessage = "Condition is required")]
    [StringLength(50, ErrorMessage = "Condition must not exceed 50 characters")]
    public string Condition { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "Pressure must be between 0 and 100000 hPa")]
    public double Pressure { get; set; }

    [Range(0, 100, ErrorMessage = "Visibility must be between 0 and 100 km")]
    public double Visibility { get; set; }
}