using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models.DTOs;

public class WeatherRequest
{
    [Required(ErrorMessage = "Location is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s,-]+$", ErrorMessage = "Location contains invalid characters")]
    public string Location { get; set; } = string.Empty;
}