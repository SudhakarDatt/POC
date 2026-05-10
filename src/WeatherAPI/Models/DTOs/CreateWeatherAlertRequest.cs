using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Models.DTOs;

public class CreateWeatherAlertRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string AlertType { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Low|Medium|High|Critical)$")]
    public string Severity { get; set; } = "Medium";

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}