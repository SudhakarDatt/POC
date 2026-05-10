using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models;

public class WeatherRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Range(-100, 100)]
    public double Temperature { get; set; }

    [Range(0, 100)]
    public int Humidity { get; set; }

    [Range(0, 200)]
    public double WindSpeed { get; set; }

    [Required]
    public string Condition { get; set; } = string.Empty;

    public double Pressure { get; set; }
    public double Visibility { get; set; }
}