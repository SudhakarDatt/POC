using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models.DTOs;

public class ForecastRequest
{
    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z0-9\s,-]+$", ErrorMessage = "Location contains invalid characters")]
    public string Location { get; set; } = string.Empty;

    [Range(1, 14, ErrorMessage = "Days must be between 1 and 14")]
    public int Days { get; set; } = 7;
}