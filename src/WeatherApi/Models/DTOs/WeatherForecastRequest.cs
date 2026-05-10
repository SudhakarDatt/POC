using System.ComponentModel.DataAnnotations;

namespace WeatherApi.Models.DTOs;

public class WeatherForecastRequest
{
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City name must be between 2 and 100 characters")]
    public string City { get; set; } = string.Empty;

    [Range(1, 14, ErrorMessage = "Days must be between 1 and 14")]
    public int Days { get; set; } = 7;
}